using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:messages", "Get messages from stream")]
        [DreamFeatureParam("limit", "int?", "Max messages to receive ranging 1 - 100 (default: 100)")]
        [DreamFeatureParam("offset", "int?", "Receive messages starting with offset (default: none)")]
        [DreamFeatureParam("start", "string?", "Oldest message timestamp to receive in format: yyyy-MM-ddTHH:mm:ssZ (default: now)")]
        [DreamFeatureParam("end", "string?", "Newest message timestamp to receive in format: yyyy-MM-ddTHH:mm:ssZ (default: +1 hour)")]
        protected Yield GetMessages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var count = context.GetParam("limit", 100);
            var offset = context.GetParam("offset", 0);
            var start = context.GetParam("start", null);
            var end = context.GetParam("end", null);
            DateTime startTime, endTime;
            if(!string.IsNullOrEmpty(end)) {
                try {
                    endTime = DateTime.Parse(end);
                } catch {
                    throw new DreamException("\"end\" is not a valid datetime parameter");
                }    
            } else {
                endTime = DateTime.UtcNow;
            }
            if(!string.IsNullOrEmpty(start)) {
                try {
                    startTime = DateTime.Parse(start);
                } catch {
                    throw new DreamException("\"start\" is not a valid datetime parameter");
                }    
            } else {
                startTime = endTime.AddHours(-1);
            }
            var messages = _data.GetMessageStream(UsivityContext.Current.Organization)
                .GetStream(startTime, endTime, count, offset);
            var doc = new XDoc("messages")
                .Attr("count", messages.Count())
                .Attr("href", _messagesUri);
            foreach(var message in messages) {
                doc.Add(GetMessageXml(message));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:messages/{messageid}", "Get message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        protected Yield GetMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var message = _data.GetMessageStream(UsivityContext.Current.Organization)
                .Get(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var doc = GetMessageVerboseXml(message);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:messages/{messageid}", "Delete message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        protected Yield DeleteMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var messages = _data.GetMessageStream(UsivityContext.Current.Organization);
            var message = messages.Get(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            messages.Delete(message);
            response.Return(DreamMessage.Ok());
            yield break;
        }
       
        [DreamFeature("POST:messages/{messageid}", "Post message in reply")]
        [DreamFeatureParam("message", "string", "Message id to reply to")]
        protected Yield PostMessageReply(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = UsivityContext.Current.Organization;
            var messages = _data.GetMessageStream(organization);
            var message = messages.Get(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }

            var connection = organization.GetConnectionReceipient(message);
            if(connection == null) {
                connection = organization.GetDefaultConnection(message.Source);
            }
            if(connection == null || connection.Identity == null) {
                throw new DreamInternalErrorException(
                    string.Format("A \"{0}\" source connection is not configured", message.Source)
                    );
            }
            if(connection.Identity.Id == message.Author.Id) {
                response.Return(DreamMessage.BadRequest("You cannot post a reply to your own message"));
                yield break;       
            }

            Message reply = null;
            try {
                reply = connection.PostReplyMessage(message, UsivityContext.Current.User, request.ToText());
            }
            catch(DreamException e) {
                response.Return(DreamMessage.InternalError("There was a problem posting the reply: " + e.Message));
            }
            messages.Save(reply);
            var doc = GetMessageVerboseXml(reply);

            var contact = _data.Contacts.Get(message);
            if(contact == null) {
                contact = new Contact();
                contact.SetIdentity(message.Source, message.Author);
                doc["message.parent"].Add(GetContactXml(contact));
            }
            contact.AddOrganization(UsivityContext.Current.Organization);
            _data.Contacts.Save(contact);

            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        //--- Methods ---
        private XDoc GetMessageXml(Message message, string relation = null) {
            var doc = message.ToDocument(relation).Attr("href", _messagesUri.At(message.Id));
            var contact = _data.Contacts.Get(message);
            if(contact != null) {
                doc.AddAll(GetContactXml(contact, "author"));
            }
            if(message.UserId != null) {
                var user = _data.Users.Get(message.UserId);
                if(user != null) {
                    doc.AddAll(GetUserXml(user, "author"));
                }
            }
            return doc;
        }            

        private XDoc GetMessageVerboseXml(Message message) {
            var doc = GetMessageXml(message);
            var parentsDoc = GetMessageParentsXml(message);
            if(!parentsDoc.IsEmpty) {
                doc.AddAll(parentsDoc);
            }
            var childrenDoc = GetMessageChildrenXml(message);
            if(!childrenDoc.IsEmpty) {
                doc.AddAll(childrenDoc);
            }
            return doc;
        }

        private XDoc GetMessageParentsXml(Message message) {
            if(message.ParentMessageId == null) {
                return XDoc.Empty;
            }
            var parent = _data.GetMessageStream(UsivityContext.Current.Organization)
                .Get(message.ParentMessageId);
            if(parent == null) {
                return XDoc.Empty;
            }
            var doc = GetMessageXml(parent, "parent");
            doc.AddAll(GetMessageParentsXml(parent));
            return doc;
        }

        private XDoc GetMessageChildrenXml(Message message) {
            var children = _data.GetMessageStream(UsivityContext.Current.Organization)
                .GetChildren(message);
            if(children.Count() <= 0) {
                return XDoc.Empty;
            }
            var doc = new XDoc("messages.children");
            foreach(var child in children) {
                doc.AddAll(GetMessageXml(child))
                    .AddAll(GetMessageChildrenXml(child));
            }
            return doc;
        }
    }
}
