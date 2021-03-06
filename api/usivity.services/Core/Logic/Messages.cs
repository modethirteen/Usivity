﻿using System;
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
using Usivity.Util;

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
        private readonly IAvatarHelper _avatarHelper;

        //--- Constructors ---
        public Messages(
            IUsivityDataCatalog data,
            ICurrentContext context,
            IContacts contacts,
            IUsers users,
            IOrganizations organizations,
            IConnections connections,
            ITwitterClientFactory twitterClientFactory,
            IEmailClientFactory emailClientFactory,
            IAvatarHelper avatarHelper
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
            _avatarHelper = avatarHelper;
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
            var resource = "message";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            XDoc author = null;
            if(message.UserId != null) {
                var user = _data.Users.Get(message.UserId);
                if(user != null) {

                    // author is usivity user
                    author = _users.GetUserXml(user, "author");
                }
            }
            if(author == null) {
                var contact = _data.Contacts.Get(message, _organizations.CurrentOrganization);
                if(contact != null) {

                    // author is contact
                    author = _contacts.GetContactXml(contact, "author");
                }
                else {

                    // author is unaffiliated
                    author = new XDoc("author")
                        .Attr("id", message.Author.Id)
                        .Elem("name", message.Author.Name ?? message.Author.Id);
                    var avatar = message.Source == Source.Email
                        ? _avatarHelper.GetGravatarUri(message.Author.Id)
                        : _avatarHelper.GetAvatarUri(message.Author);
                    author.Elem("uri.avatar", (avatar != null) ? avatar.ToString() : "");
                }    
            }
            return new XDoc(resource)
                .Attr("id", message.Id)
                .Attr("href", _context.ApiUri.At("messages", message.Id))
                .Elem("source", message.Source.ToString().ToLowerInvariant())
                .AddAll(author)
                .Elem("subject", message.Subject)
                .Elem("body", message.Body)
                .Elem("created.source", message.SourceCreated.ToISO8601String())
                .Elem("created.openstream", message.Created.ToISO8601String());
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
            var contact = _data.Contacts.Get(message, organization);
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

            // build child messages document
            var doc = new XDoc("messages.children");
            foreach(var child in children) {
                var childDoc = GetMessageXml(child); 
                if(!tree) {
                    childDoc.Attr("depth", depth);
                }
                doc.AddAll(childDoc)
                    .AddAll(GetMessageChildrenXml(child, depth + 1, tree));
            }

            // flatten message nodes for total tree message count
            var flattenedMessageNodes = doc[".//message"];
            doc.EndAll()
                .Attr("count", children.Count())
                .Attr("totalcount", flattenedMessageNodes.ListLength);
            if(tree) {

                // return nested messages
                return doc;
            }

            // return flattened messages
            doc.RemoveNodes().AddAll(flattenedMessageNodes);
            return (depth == 0) ? doc : flattenedMessageNodes;
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
