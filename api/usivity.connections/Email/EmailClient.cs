using System;
using System.Collections.Generic;
using System.Net.Mail;
using AE.Net.Mail;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Connections.Email {

    public class ImapClientConfig {

        //--- Properties ---
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public ImapClient.AuthMethods AuthMethod { get; set; }
        public bool UseSsl { get; set; }
    }

    public class SmtpClientConfig {
        
    }

    public class EmailClient : IConnection {

        //--- Constants ---
        private const string IN_REPLY_TO_HEADER = "In-Reply-To";

        //--- Properties ---
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public Source Source { get; private set; }
        public Identity Identity { get; private set; }
        public bool Active { get
            { return Identity != null && _imapConfig != null; }
        }

        //--- Fields ---
        private ImapClient _imap;
        private ImapClientConfig _imapConfig;
        private SmtpClient _smtp;
        private SmtpClientConfig _smtpConfig;
        private DateTime _lastSearch;

        //--- Constructors ---
        public EmailClient(IGuidGenerator guidGenerator, IOrganization organization, XDoc config) {
            Id = guidGenerator.GenerateNewObjectId();
            OrganizationId = organization.Id;
            Source = Source.Email;
        }
        
        //--- Methods ---
        public IEnumerable<IMessage> GetMessages(IGuidGenerator guidGenerator, IDateTime dateTime, TimeSpan? expiration = null) {
            var imap = GetImapClient();
            var search = new SearchCondition {
                Field = SearchCondition.Fields.Since,
                Value = _lastSearch.ToLocalTime()
            }
            .And(SearchCondition.Unseen());
            _lastSearch = DateTime.UtcNow;
            var mailMessages = imap.SearchMessages(search);

            var messages = new List<IMessage>();
            foreach(var mailMessageDeferred in mailMessages) {
                var mailMessage = mailMessageDeferred.Value;
                imap.AddFlags(Flags.Seen, mailMessage);

                var identity = new Identity();
                var sender = mailMessage.From ?? mailMessage.Sender;
                if(sender != null) {
                    identity.Id = sender.Address;
                    identity.Name = sender.DisplayName;
                }
                var message = new Message(guidGenerator, dateTime, expiration) {
                    Source = Source.Email,
                    SourceMessageId = mailMessage.MessageID,
                    SourceInReplyToMessageId = mailMessage.Headers[IN_REPLY_TO_HEADER].Value,
                    Body = mailMessage.Body,
                    Subject = mailMessage.Subject,
                    Author = identity,
                    SourceTimestamp = mailMessage.Date,
                    OpenStream = true
                };
                messages.Add(message);
            }
            imap.Dispose();
            return messages;
        }

        public IMessage PostReplyMessage(IGuidGenerator guidGenerator, IDateTime dateTime, IMessage message, User user, string reply) {
            throw new NotImplementedException();
        }

        public XDoc ToDocument() {
            var doc = new XDoc("connection")
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .Elem("type", "imap");
            if(Active) {
                doc.Start("identity")
                    .Attr("id", Identity.Id)
                    .Elem("name", Identity.Name)
                .End()
                .Elem("active", true);
            }
            else {
                doc.Elem("active", false);
            }
            return doc; 
        }

        public void Update(XDoc config) {
            if(config["imap"].IsEmpty) {
                var response = DreamMessage.BadRequest("Connection update document is missing an \"imap\" element");
                throw new DreamAbortException(response); 
            }
            var address = config["imap/email"].AsText;
            if(string.IsNullOrEmpty(address)) {
                var response = DreamMessage.BadRequest("Connection update document is missing an \"imap/email\" element");
                throw new DreamAbortException(response); 
            }
            var imapConfig = new ImapClientConfig {
                Host = config["imap/host"].AsText,
                Username = config["imap/username"].AsText ?? address,
                Password = config["imap/password"].AsText,
                Port = config["imap/port"].AsInt ?? 143,
                UseSsl = config["imap/ssl"].AsBool ?? false
            };
            var auth = config["imap/auth.method"].AsText;
            if(!string.IsNullOrEmpty(auth)) {
                auth = auth.ToLowerInvariant();
            }
            switch(auth) {
                case "crammd5":
                    imapConfig.AuthMethod = ImapClient.AuthMethods.CRAMMD5;
                    break;
                case "login":
                case null:
                    imapConfig.AuthMethod = ImapClient.AuthMethods.Login;
                    break;
                default:
                    var response = DreamMessage
                        .BadRequest("Connection update document \"auth.method\" element is invalid. Allowed values: login, crammd5");
                    throw new DreamAbortException(response);
            }
            _imapConfig = imapConfig;
            var imap = GetImapClient();
            if(!imap.IsAuthenticated) {
                var response = DreamMessage.BadRequest("Could not authenticate with IMAP server");
                throw new DreamAbortException(response);
            }
            Identity = new Identity {
                Id = address
            };
            _imap = imap;
        }

        private ImapClient GetImapClient() {
            if((_imap == null && _imapConfig != null) || (_imap != null && _imap.IsDisposed)) {
                var c = _imapConfig;
                _imap = new ImapClient(c.Host, c.Username, c.Password, c.AuthMethod, c.Port, c.UseSsl, true);
                _imap.SelectMailbox("INBOX");
            }
            return _imap;
        }
    }
}
