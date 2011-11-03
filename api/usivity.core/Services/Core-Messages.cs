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
        [DreamFeatureParam("stream", "{open,user}", "Message stream to fetch from")]
        [DreamFeatureParam("limit", "int?", "Max messages to receive ranging 1 - 100 (default: 10)")]
        [DreamFeatureParam("offset", "int?", "Max messages to receive ranging 1 - 100 (default: 10)")]
        protected Yield GetMessages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var count = context.GetParam("limit", 10);
            var stream = context.GetParam<string>("stream").ToLowerInvariant();

            IEnumerable<Message> messages;
            switch(stream) {
                case "user":
                    messages = _data
                        .GetUserStreamMessages(UsivityContext.Current.Organization, UsivityContext.Current.User, count);
                    break;
                default:
                    messages = _data
                        .GetOpenStreamMessages(UsivityContext.Current.Organization, UsivityContext.Current.User, count, _refresh);
                    break;
            }
            var doc = new XDoc("messages")
                .Attr("count", messages.Count())
                .Attr("href", _messagesUri);
            foreach(var message in messages) {
                doc.Add(GetMessageChildrenXml(message));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:messages/{messageid}", "Get message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        protected Yield GetMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var message = _data
                .GetMessage(UsivityContext.Current.Organization, UsivityContext.Current.User, context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var doc = GetMessageXml(message);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:messages/{messageid}", "Delete message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        protected Yield DeleteMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var message = _data
                .GetMessage(UsivityContext.Current.Organization, UsivityContext.Current.User, context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            _data.DeleteMessage(UsivityContext.Current.Organization, message.Id);
            response.Return(DreamMessage.Ok());
            yield break;
        }
       
        [DreamFeature("POST:messages/{messageid}", "Post message in reply")]
        [DreamFeatureParam("message", "string", "Message id to reply to")]
        protected Yield PostMessageReply(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var message = _data
                .GetMessage(UsivityContext.Current.Organization, UsivityContext.Current.User, context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }

            var source = _sources.FirstOrDefault(s => s.Id== message.Source);
            if(source == null) {
                throw new DreamInternalErrorException("Could not locate requested message's source network, \"" + message.Source + "\"");
            }

            var connection = UsivityContext.Current.User.GetConnection(source.Id);
            if(connection == null) {
                response.Return(DreamMessage.BadRequest("You have not connected to source network \"" + source.Id + "\""));
                yield break;
            }

            var userSourceIdentity = UsivityContext.Current.User.GetConnection(source.Id).Identity;
            if(userSourceIdentity != null && userSourceIdentity.Id == message.Author.Id) {
                response.Return(DreamMessage.BadRequest("You cannot post a reply to your own message"));
                yield break;       
            }
            
            Message reply = null;
            try {
                reply = source.PostMessageReply(message, request.ToText(), connection);
            }
            catch(DreamException e) {
                response.Return(DreamMessage.InternalError(
                    string.Format("There was a problem posting the reply. Source network \"{0}\" returned the following message: {1}",
                        source.Id,
                        e.Message
                    )
                ));
            }
            if(reply == null) {
                throw new DreamInternalErrorException(string.Format("Unable to construct message object from source network \"{0}\" response", source.Id));
            }
            _data.SaveMessage(reply, UsivityContext.Current.Organization);

            var doc = GetMessageParentXml(reply);

            var contact = _data.GetContact(message);
            if(contact == null) {
                contact = new Contact(UsivityContext.Current.User);
                contact.SetSourceIdentity(message.Source, message.Author);
                var contactMessages = _data.GetMessages(UsivityContext.Current.Organization, contact);
                foreach(var contactMessage in contactMessages) {
                    contactMessage.MoveToStream(Message.MessageStreams.User);
                    _data.SaveMessage(contactMessage, UsivityContext.Current.Organization);
                }
                
                // new contact means new conversation
                contact.AddConversationMessageId(message.Id);
                _data.SaveContact(contact);
                doc["message.parent/author"].Add(GetContactXml(contact, "new"));
            }

            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        //--- Methods ---
        private XDoc GetMessageXml(Message message, string relation = null) {
            return message.ToDocument(relation).Attr("href", _messagesUri.At(message.Id));
        }

        private XDoc GetMessageParentXml(Message message) {
            var doc = GetMessageXml(message);
            if(message.ParentMessageId != null) {
                var parent = _data.GetMessage(UsivityContext.Current.Organization, message.ParentMessageId);
                if(parent != null) {
                    doc.AddAll(GetMessageXml(parent, "parent"));
                }
            }
            return doc;
        }

        private XDoc GetMessageChildrenXml(Message message) {
            var doc = GetMessageXml(message);
            var children = _data.GetMessageChildren(UsivityContext.Current.Organization, message);
            if(children.Count() > 0) {
                doc.Start("messages.children");
                foreach(var child in children) {
                    doc.AddAll(GetMessageChildrenXml(child));
                }
            }
            return doc;
        }
    }
}
