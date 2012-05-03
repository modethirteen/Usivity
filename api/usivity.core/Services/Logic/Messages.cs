using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services.Logic {

    public class Messages : IMessages {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IMessageDataAccess _messageStream;
        private readonly IContacts _contacts;
        private readonly IUsers _users;
        private readonly IOrganizations _organizations;
        private readonly IConnections _connections;

        //--- Constructors ---
        public Messages(IUsivityDataCatalog data, ICurrentContext context, IContacts contacts, IUsers users, IOrganizations organizations, IConnections connections) {
            _context = context;
            _data = data;
            var organization = organizations.CurrentOrganization;
            _messageStream = _data.GetMessageStream(organization);
            _contacts = contacts;
            _users = users;
            _organizations = organizations;
            _connections = connections;
        }

        //--- Methods ---
        public Message GetMessage(string id) {
            return _messageStream.Get(id);
        }

        public XDoc GetConversationsXml(Contact contact) {
            var messages = _messageStream.GetConversations(contact);
            var doc = new XDoc("messages");
            foreach(var message in messages) {
                doc.AddAll(GetMessageXml(message))
                    .AddAll(GetMessageChildrenXml(message));
            }
            return doc;
        }

        public XDoc GetMessageXml(Message message, string relation = null) {
            var doc = message.ToDocument(relation)
                .Attr("href", _context.ApiUri.At("messages", message.Id));
            var contact = _data.Contacts.Get(message);
            if(contact != null) {
                doc.AddAll(_contacts.GetContactXml(contact, "author"));
            }
            if(message.UserId != null) {
                var user = _data.Users.Get(message.UserId);
                if(user != null) {
                    doc.AddAll(_users.GetUserXml(user, "author"));
                }
            }
            return doc;
        }            

        public XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset, Source source) {
            var messages = _messageStream.GetStream(startTime, endTime, count, offset, source);
            var doc = GetMessagesXml(messages);
            return doc;
        }

        public XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset) {
            var messages = _messageStream.GetStream(startTime, endTime, count, offset);
            var doc = GetMessagesXml(messages);
            return doc;
        }

        public XDoc GetMessageVerboseXml(Message message) {
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

        public XDoc GetMessageParentsXml(Message message) {
            if(message.ParentMessageId == null) {
                return XDoc.Empty;
            }
            var parent = _messageStream.Get(message.ParentMessageId);
            if(parent == null) {
                return XDoc.Empty;
            }
            var doc = GetMessageXml(parent, "parent");
            doc.AddAll(GetMessageParentsXml(parent));
            return doc;
        }

        public XDoc GetMessageChildrenXml(Message message) {
            var children = _messageStream.GetChildren(message);
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

        public XDoc PostReply(Message message, string reply) {
            var organization = _organizations.CurrentOrganization;
            var connection = _connections.GetConnectionReceipient(message) ?? _connections.GetDefaultConnection(message.Source);
            if(connection == null || connection.Identity == null) {
                var response = DreamMessage.Forbidden(string.Format("A \"{0}\" source connection has not been configured", message.Source));
                throw new DreamAbortException(response);
            }
            if(connection.Identity.Id == message.Author.Id) {
                var response = DreamMessage.BadRequest("You cannot post a reply to your own message");
                throw new DreamAbortException(response);
            }

            Message replyMessage;
            try {
                replyMessage = connection.PostReplyMessage(message, _context.User, reply);
            }
            catch(Exception e) {
                var response = DreamMessage.InternalError("There was a problem posting the reply: " + e.Message);
                throw new DreamAbortException(response); 
            }
            _messageStream.Save(replyMessage);
            var doc = GetMessageVerboseXml(replyMessage);
            var contact = _data.Contacts.Get(message);
            if(contact == null) {
                contact = new Contact();
                contact.SetIdentity(message.Source, message.Author);
                doc["message.parent"].Add(_contacts.GetContactXml(contact));
            }
            contact.AddOrganization(organization);
            _data.Contacts.Save(contact);
            return doc;
        }

        public void SaveMessage(Message message) {
            _messageStream.Save(message);
        }

        public void DeleteMessage(Message message) {
            _messageStream.Delete(message);
        }

        private XDoc GetMessagesXml(IEnumerable<Message> messages) {
            var doc = new XDoc("messages")
                .Attr("count", messages.Count())
                .Attr("href", _context.ApiUri.At("messages"));
            foreach(var message in messages) {
                doc.Add(GetMessageXml(message));
            }
            doc.EndAll();
            return doc;
        }
    }
}
