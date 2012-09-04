using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.OAuth;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Services.Clients;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.OAuth;
using Usivity.Services.Clients.Twitter;
using Usivity.Services.Parser;
using Usivity.Util;

namespace Usivity.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity OpenStream Queue API Service", "", SID = new[] { "sid://usivity.com/2012/04/openstream" })]
    public class OpenStreamService : DreamService {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Class Methods ---
        private static IDateTime NewDateTime() {
            return new DateTimeImpl();
        }

        //--- Fields ---
        private readonly IGuidGenerator _guidGenerator = new GuidGenerator();
        private IUsivityDataCatalog _data;
        private string _twitterConsumerKey;
        private string _twitterConsumerSecret;
        private string _awsPublicKey;
        private string _awsPrivateKey;
        private long _messageCount;
        private TimeSpan _messageExpiration;

        //--- Features ---
        [DreamFeature("GET:status", "Get openstream queue service status")]
        public Yield GetStatus(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = new XDoc("status").Elem("messages.count", _messageCount);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        //--- Methods ---
        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            _log.Debug("Initializing data catalog connection");
            var dbConnection = config["mongodb/connection"].AsText;
            if(string.IsNullOrEmpty(dbConnection)) {
                throw new DreamInternalErrorException("Database connection string required");
            }
            _data = UsivityDataCatalog.NewUsivityDataCatalog(dbConnection);
            _twitterConsumerKey = config["sources/twitter/consumer.key"].AsText;
            _twitterConsumerSecret = config["sources/twitter/consumer.secret"].AsText;
            _awsPublicKey = config["aws/public.key"].AsText;
            _awsPrivateKey = config["aws/private.key"].AsText;
            var ms = double.MinValue;
            double.TryParse(config["openstream/expiration"].AsText, out ms);

            _log.DebugFormat("Setting message expiration to {0}ms", _messageExpiration);
            _messageExpiration = TimeSpan.FromMilliseconds(ms);

            _log.Debug("Starting open stream queue");
            TaskTimerFactory.Current.New(TimeSpan.Zero, QueueMessages, null, TaskEnv.None);
            TaskTimerFactory.Current.New(TimeSpan.Zero, UpdateMessagesCount, null, TaskEnv.None);
            result.Return();
        }

        private void UpdateMessagesCount(TaskTimer tt) {
            var organizations = _data.Organizations.Get();
            _messageCount = organizations
                .Aggregate<IOrganization, long>(0, (current, organization) => current + _data.GetMessageStream(organization).GetCount());
            tt.Change(TimeSpan.FromMinutes(1), TaskEnv.None);
        }

        private void QueueMessages(TaskTimer tt) {
            foreach(var organization in _data.Organizations.Get()) {
                _data.GetMessageStream(organization).RemoveExpired(NewDateTime());
                Coroutine.Invoke(QueueOrganizationMessages, organization, new Result());    
            }
            tt.Change(TimeSpan.FromMinutes(1), TaskEnv.None);
        }

        private Yield QueueOrganizationMessages(IOrganization organization, Result result) {
            var messages = new List<IMessage>();
            var connections = _data.Connections.Get(organization);
            foreach(var subscription in _data.Subscriptions.Get(organization)) {

                // if any organization twitter connections exist get public messages);
                var connection = connections.Where(c => c.Source == Source.Twitter).FirstOrDefault();
                var twitterConnection = connection as TwitterConnection;
                if(twitterConnection != null) {
                    var twitterClient = NewTwitterClient(twitterConnection);
                    var constraints = string.Join(",", subscription.Constraints.ToArray());
                    _log.DebugFormat("Fetching Twitter messages for organization {0}, subscription: {1}", organization.Name, constraints);
                    try {
                        var fetchedMessages = twitterClient.GetNewPublicMessages(subscription, _messageExpiration);
                        messages.AddRange(fetchedMessages);
                        _log.DebugFormat("Recieved {0} messages for organization {1}", fetchedMessages.Count(), organization.Name);
                    } catch(Exception e) {
                        _log.WarnFormat("Could not receive messages for organization {0}, exception: {1}", organization.Name, e);    
                    }
                    _data.Subscriptions.Save(subscription);
                }
            }

            // queue private and direct messages
            foreach (var connection in connections) {
                _log.DebugFormat(
                    "Fetching {0} messages for organization {1}, identity: {2}", connection.Source, organization.Name, connection.Identity.Id
                );
                try {
                    var client = NewClient(connection);
                    var fetchedMessages = client.GetNewMessages(_messageExpiration);
                    messages.AddRange(fetchedMessages);
                    _log.DebugFormat("Received {0} messages for organization {1}", fetchedMessages.Count(), organization.Name);
                } catch(Exception e) {
                    _log.WarnFormat("Could not receive messages for organization {0}, exception: {1}", organization.Name, e.Message);    
                }
                _data.Connections.Save(connection);
            }
            
            var span = NewDateTime().UtcNow.Subtract(_messageExpiration);
            var stream = _data.GetMessageStream(organization);
            _log.DebugFormat("Queueing {0} messages for organization {1}", messages.Count, organization.Name);
            foreach(var message in messages.Where(message => message.Created >= span)) {
                if(stream.Get(message.Source, message.SourceMessageId) != null) {

                    // message is already in openstream
                    continue;
                }
                try {
                    var parser = new MessageContentParser(message);
                    parser.Process();
                    var parsedContent = parser.MessageContent;
                    if(!string.IsNullOrEmpty(parsedContent)) {
                        message.Body = parsedContent;
                    }
                } catch(Exception e) {
                    _log.WarnFormat("Could not parse {0} message content for id {1}, exception: {2}", message.Source, message.Id, e.Message);
                }
                stream.Queue(message);
            }
            _log.DebugFormat("Finished queueing messages for organization {0}", organization.Name);
            result.Return();
            yield break;
        }

        private IClient NewClient(IConnection connection) {
            var twitterConnection = connection as TwitterConnection;
            if(twitterConnection != null) {
                return NewTwitterClient(twitterConnection);
            }
            var emailConnection = connection as EmailConnection;
            if(emailConnection != null) {
                return NewEmailClient(emailConnection);
            }
            throw new NotSupportedException("Source connection client is not supported");
        }

        private ITwitterClient NewTwitterClient(ITwitterConnection connection) {
            var config = new OAuthConfig {
                ConsumerKey = _twitterConsumerKey,
                ConsumerSecret = _twitterConsumerSecret,
                NonceFactory = new OAuthNonceFactory(),
                TimeStampFactory = new OAuthTimeStampFactory()
            };
            var factory = new TwitterClientFactory(config, _guidGenerator, NewDateTime());
            return factory.NewTwitterClient(connection);
        }

        private IEmailClient NewEmailClient(IEmailConnection connection) {
            var config = new SimpleEmailServiceConfig {
                AwsPublicKey = _awsPublicKey,
                AwsPrivateKey = _awsPrivateKey
            };
            var factory = new EmailClientFactory(config, _guidGenerator, NewDateTime());
            return factory.NewEmailClient(connection); 
        }
    }
}
