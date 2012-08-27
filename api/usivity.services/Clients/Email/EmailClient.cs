using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using AE.Net.Mail;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Util;
using MailMessage = System.Net.Mail.MailMessage;

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
        private readonly SmtpClient _smtp;
        private ImapClient _imap;

        //--- Constructors ---
        public EmailClient(EmailClientConfig config, IEmailConnection connection, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _connection = connection;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
            _smtp = new SmtpClient {
                Host = config.Host,
                EnableSsl = config.UseSsl,
                Port = config.Port != int.MinValue ? config.Port : 25
            };
            if(!string.IsNullOrEmpty(config.Username)) {
                var credentials = new NetworkCredential(config.Username, config.Password);
                _smtp.Credentials = credentials;
            }
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
            var replyAddress = message.Author.Id;
            var email = new MailMessage {
                From = new MailAddress(_connection.Identity.Id)
            };
            email.To.Add(replyAddress);
            var subject = message.Subject;
            if(!subject.StartsWith("Re:")) {
                subject = "Re:" + subject;
            }
            email.Subject = subject;
            _smtp.Send(email);

            // TODO: add SourceMessageId and SourceInReplyToMessageId
            var replyMessage = new Message(_guidGenerator, _dateTime) {
                Source = Source.Email,
                SourceInReplyToIdentityId = replyAddress,
                Subject = subject,
                Body = reply,
                Author = _connection.Identity,
                SourceCreated = _dateTime.UtcNow,
                UserId = user.Id
            };
            replyMessage.SetParent(message);
            return replyMessage;
        }

        public Contact NewContact(Identity identity) {
            var contact = new Contact(_guidGenerator) {
                Avatar = identity.Avatar
            };
            contact.SetIdentity(Source.Email, identity);
            return contact;
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
