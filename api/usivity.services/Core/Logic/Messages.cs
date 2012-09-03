using System;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Services.Clients;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.Twitter;

namespace Usivity.Services.Core.Logic {

    public class Messages : IMessages {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IMessageDataAccess _messageStream;
        private readonly IContacts _contacts;
        private readonly IUsers _users;
        private readonly IOrganizations _organizations;
        private readonly IConnections _connections;
        private readonly ITwitterClientFactory _twitterClientFactory;
        private readonly IEmailClientFactory _emailClientFactory;

        //--- Constructors ---
        public Messages(
            IUsivityDataCatalog data,
            ICurrentContext context,
            IContacts contacts,
            IUsers users,
            IOrganizations organizations,
            IConnections connections,
            ITwitterClientFactory twitterClientFactory,
            IEmailClientFactory emailClientFactory
        ) {
            _context = context;
            _data = data;
            var organization = organizations.CurrentOrganization;
            _messageStream = _data.GetMessageStream(organization);
            _contacts = contacts;
            _users = users;
            _organizations = organizations;
            _connections = connections;
            _twitterClientFactory = twitterClientFactory;
            _emailClientFactory = emailClientFactory;
        }

        //--- Methods ---
        public IMessage GetMessage(string id) {
            return _messageStream.Get(id);
        }

        public XDoc GetConversationsXml(Contact contact, bool renderChildrenAsTree = true) {
            var messages = _messageStream.GetConversations(contact);
            var doc = new XDoc("messages").Attr("count", messages.Count());
            foreach(var message in messages) {
                doc.AddAll(GetMessageXml(message)
                    .AddAll(GetMessageChildrenXml(message, 0, renderChildrenAsTree)));
            }
            doc.Attr("totalcount", doc[".//message"].ListLength);
            return doc;
        }

        public XDoc GetMessageXml(IMessage message, string relation = null) {
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

        public XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset, Source? source) {
            _log.DebugFormat("Getting {0} {1} messages, offset by {2}, from openstream from {3} to {4}",
                count, source != null ? source.ToString() : "all", offset, startTime, endTime);
            var messages = _messageStream.GetStream(startTime, endTime, count, offset, source);
            var messagesCount = messages.Count();
            _log.DebugFormat("Received {0} messages from openstream", messagesCount);
            var doc = new XDoc("messages")
                .Attr("count", messagesCount)
                .Attr("href", _context.ApiUri.At("messages"));
            foreach(var message in messages) {
                doc.Add(GetMessageXml(message));
            }
            doc.EndAll();
            return doc;
        }

        public XDoc GetMessageVerboseXml(IMessage message, bool renderChildrenAsTree = true) {
            var doc = GetMessageXml(message);
            var parentsDoc = GetMessageParentsXml(message);
            if(!parentsDoc.IsEmpty) {
                doc.AddAll(parentsDoc);
            }
            var childrenDoc = GetMessageChildrenXml(message, 0, renderChildrenAsTree);
            if(!childrenDoc.IsEmpty) {
                doc.AddAll(childrenDoc);
            }
            return doc;
        }

        public XDoc GetMessageParentsXml(IMessage message) {
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

        public XDoc PostReply(IMessage message, string reply) {
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
            var client = NewClient(connection);
            var replyMessage = client.PostNewReplyMessage(_context.User, message, reply);
            replyMessage.OpenStream = false;

            // once a message has been replied to, it should not be purged from the stream
            if(message.Expires != null) {
                message.RemoveExpiration();
                _messageStream.Save(message);
            }
            _messageStream.Save(replyMessage);
            var doc = GetMessageVerboseXml(replyMessage);
            var contact = _data.Contacts.Get(message);
            if(contact == null) {
                contact = client.NewContact(message.Author);
                doc["message.parent"].Add(_contacts.GetContactXml(contact));
            }
            contact.AddOrganization(organization);
            _data.Contacts.Save(contact);
            return doc;
        }

        public void SaveMessage(IMessage message) {
            _messageStream.Save(message);
        }

        public void DeleteMessage(IMessage message) {
            _messageStream.Delete(message);
        }

        private XDoc GetMessageChildrenXml(IMessage message, int depth, bool tree = true) {
            var children = _messageStream.GetChildren(message);
            if(!children.Any()) {
                return XDoc.Empty;
            }
            var doc = new XDoc("messages.children").Attr("count", children.Count());
            foreach(var child in children) {
                var childDoc = GetMessageXml(child); 
                if(!tree) {
                    childDoc.Attr("depth", depth);
                }
                doc.AddAll(childDoc)
                    .AddAll(GetMessageChildrenXml(child, depth + 1, tree));
            }
            var flatChildMessages = doc[".//message"];
            var totalCount = flatChildMessages.ListLength;
            doc.EndAll();
            if(tree) {
                return doc.Attr("totalcount", totalCount);
            }
            return depth == 0
                ? new XDoc("messages.children")
                    .Attr("count", children.Count())
                    .Attr("totalcount", totalCount)
                    .AddAll(flatChildMessages)
                : flatChildMessages; 
        }

        private IClient NewClient(IConnection connection) {
            var twitterConnection = connection as TwitterConnection;
            if(twitterConnection != null) {
                return _twitterClientFactory.NewTwitterClient(twitterConnection);
            }
            var emailConnection = connection as EmailConnection;
            if(emailConnection != null) {
                return _emailClientFactory.NewEmailClient(emailConnection); 
            }
            throw new NotSupportedException("Source connection client is not supported");
        }
    }
}
