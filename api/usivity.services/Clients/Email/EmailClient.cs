using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using AE.Net.Mail;
using log4net;
using MindTouch;
using MindTouch.Dream;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Services.Clients.Email {

    public interface IEmailClient : IClient {}

    public class EmailClient : IEmailClient {

        //--- Constants ---
        private const string IN_REPLY_TO_HEADER = "In-Reply-To";
        private const string AMAZON_SES_HOST = "https://email.us-east-1.amazonaws.com";

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Class Methods ---
        public static Identity NewIdentityFromEmailAddress(string email) {
            return new Identity { Id = email.ToLowerInvariant() };
        }

        public static void CheckEmailConnectionCredentials(IEmailConnection connection) {
            try {
                NewImapClient(connection);
            }
            catch(Exception e) {
                throw new ConnectionResponseException(DreamStatus.BadRequest, "Could not successfully validate email connection settings", e);
            }
        }

        public static bool IsValidEmailAddress(string email) {
            return email.Contains("@");
        }

        private static ImapClient NewImapClient(IEmailConnection connection) {
            var imap = new ImapClient(
                connection.Host,
                connection.Username,
                connection.Password,
                connection.UseCramMd5 ? ImapClient.AuthMethods.CRAMMD5 : ImapClient.AuthMethods.Login,
                connection.Port,
                connection.UseSsl,
                true
                );
            imap.SelectMailbox("INBOX");
            return imap;
        }

        //--- Fields ---
        private readonly IEmailConnection _connection;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTime _dateTime;
        private readonly string _awsPublicKey;
        private readonly string _awsPrivateKey;

        //--- Constructors ---
        public EmailClient(SimpleEmailServiceConfig config, IEmailConnection connection, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _connection = connection;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
            _awsPublicKey = config.AwsPublicKey;
            _awsPrivateKey = config.AwsPrivateKey;
        }

        //--- Methods ---
        #region IClient implementation
        public IEnumerable<IMessage> GetNewMessages(TimeSpan? expiration) {
            var imap = NewImapClient(_connection);

            // build search conditions; unseen messages since connection creation -1 day
            var search = SearchCondition.Unseen();
            var since = _connection.Created.Subtract(TimeSpan.FromDays(1));
            search = search.And(new SearchCondition {
                    Field = SearchCondition.Fields.Since,
                    Value = since.ToString("dd-MMM-yyyy").QuoteString()
            });
            var mailMessages = imap.SearchMessages(search);
            var messages = new List<IMessage>();
            foreach(var mailMessageDeferred in mailMessages) {
                var mailMessage = mailMessageDeferred.Value;
                imap.AddFlags(Flags.Seen, mailMessage);

                // ignore messages older than connection
                if(mailMessage.Date < _connection.Created) {
                    continue;
                }
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
            var date = _dateTime.UtcNow.ToString("R");

            // generate auth header
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_awsPrivateKey));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(date)));
            var auth = string.Format("AWS3-HTTPS AWSAccessKeyId={0}, Algorithm=HmacSHA256, Signature={1}", _awsPublicKey, signature);

            // prepend replies with reply prefix
            var subject = message.Subject;
            if(!subject.StartsWith("Re:")) {
                subject = "Re:" + subject;
            }
            var plug = Plug.New(AMAZON_SES_HOST)
                .WithHeader("X-Amz-Date", date)
                .WithHeader("X-Amzn-Authorization", auth)
                .With("Timestamp", _dateTime.UtcNow.ToISO8601String())
                .With("AwsAccessKeyId", _awsPublicKey)
                .With("Action", "SendEmail")
                .With("Destination.ToAddresses.member.1", message.Author.Id)
                .With("Message.Body.Text.Data", reply)
                .With("Message.Subject.Data", subject)
                .With("Source", _connection.Identity.Id);
            DreamMessage response;
            try {
                response = plug.PostAsForm();
            }
            catch(DreamResponseException e) {
                throw new ConnectionResponseException(e.Response.Status, "Error posting message", plug.Uri, e.Response, e);
            }
            var sendEmailResult = response.ToDocument();
            sendEmailResult.UsePrefix("ses", "http://ses.amazonaws.com/doc/2010-12-01/");
            var replyMessage = new Message(_guidGenerator, _dateTime) {
                Source = Source.Email,
                SourceMessageId = sendEmailResult["ses:SendEmailResult/ses:MessageId"].AsText,
                SourceInReplyToMessageId = message.Id,
                SourceInReplyToIdentityId = message.Author.Id,
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
            var contact = new Contact(_guidGenerator);
            contact.SetIdentity(Source.Email, identity);
            contact.FirstName = identity.Name;
            return contact;
        }
        #endregion
    }
}
