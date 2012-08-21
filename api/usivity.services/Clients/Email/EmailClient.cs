using System;
using System.Collections.Generic;
using System.Net.Mail;
using AE.Net.Mail;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Services.Clients.Email {

    public class EmailClient : IEmailClient {

        //--- Constants ---
        private const string IN_REPLY_TO_HEADER = "In-Reply-To";

        //--- Class Methods ---
        public static Identity GetIdentityByEmailAddress(string email) {
            return new Identity {
                Id = email,
                Name = email
            };
        }

        //--- Fields ---
        private readonly IEmailConnection _connection;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTime _dateTime;
        private ImapClient _imap;
        private SmtpClient _smtp;

        //--- Constructors ---
        public EmailClient(EmailClientConfig config, IEmailConnection connection, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _connection = connection;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
            _smtp = null;
        }

        //--- Methods ---
        #region IClient implementation
        public IEnumerable<IMessage> GetNewMessages(TimeSpan? expiration) {
            var imap = GetImapClient();
            var search = SearchCondition.Unseen();
            search = search.And(new SearchCondition {
                    Field = SearchCondition.Fields.Since,
                    Value = _connection.Modified.ToLocalTime()
            });

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
                var message = new Message(_guidGenerator, _dateTime, expiration) {
                    Source = Source.Email,
                    SourceMessageId = mailMessage.MessageID,
                    SourceInReplyToMessageId = mailMessage.Headers[IN_REPLY_TO_HEADER].Value,
                    Body = mailMessage.Body,
                    Subject = mailMessage.Subject,
                    Author = identity,
                    SourceCreated = mailMessage.Date,
                    OpenStream = true
                };
                messages.Add(message);
            }
            imap.Dispose();
            return messages;
        }

        public IMessage PostNewReplyMessage(IUser user, IMessage message, string reply) {
            throw new NotImplementedException();
        }

        public Contact NewContact(Identity identity) {
            throw new NotImplementedException();
        }
        #endregion

        #region IEmailClient implementation
        public bool AreEmailConnectionCredentialsValid(out Exception clientErrorResponse) {
            clientErrorResponse = null;
            ImapClient imap = null;
            try {
                imap = GetImapClient(); 
            }
            catch(Exception e) {
                clientErrorResponse = e;
            }
            return imap != null; 
        }
        #endregion

        private ImapClient GetImapClient() {
            if(_imap == null || _imap.IsDisposed) {
                _imap = new ImapClient(
                    _connection.Host,
                    _connection.Username,
                    _connection.Password,
                    _connection.UseCramMd5 ? ImapClient.AuthMethods.CRAMMD5 : ImapClient.AuthMethods.Login,
                    _connection.Port,
                    _connection.UseSsl,
                    true
                    );
                _imap.SelectMailbox("INBOX");
            }
            return _imap;
        }
    }
}
