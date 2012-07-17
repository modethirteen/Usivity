using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Services.Clients;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.Twitter;
using Usivity.Util;

namespace Usivity.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity OpenStream Queue API Service", "", SID = new[] { "sid://usivity.com/2012/04/openstream" })]
    public class OpenStreamService : DreamService {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private readonly IGuidGenerator _guidGenerator = new GuidGenerator();
        private readonly IDateTime _dateTime = new DateTimeImpl();
        private IUsivityDataCatalog _data;
        private TwitterClientFactory _twitterClientFactory;
        private EmailClientFactory _emailClientFactory;
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

            _log.Debug("Stashing network connection settings");
            var twitterDoc = config["sources/twitter"];
            var twitterClientConfig = new TwitterClientConfig {
                OAuthConsumerKey = twitterDoc["oauth/consumer.key"].Contents,
                OAuthConsumerSecret = twitterDoc["oauth/consumer.secret"].Contents
            };
            _twitterClientFactory = new TwitterClientFactory(twitterClientConfig, _guidGenerator, _dateTime);
            var emailClientConfig = new EmailClientConfig();
            _emailClientFactory = new EmailClientFactory(emailClientConfig, _guidGenerator, _dateTime);

            var ms = double.MinValue;
            double.TryParse(config["openstream/expiration"].AsText, out ms);
            _messageExpiration = TimeSpan.FromMilliseconds(ms);
            _log.DebugFormat("Setting message expiration to {0}ms", _messageExpiration);

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
                _data.GetMessageStream(organization).RemoveExpired();
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
                    var twitterClient = _twitterClientFactory.NewTwitterClient(twitterConnection);
                    if(subscription.GetSourceUri(Source.Twitter) == null) {
                        twitterClient.SetNewSubscriptionQuery(subscription);
                    }
                    var constraints = string.Join(",", subscription.Constraints.ToArray());
                    _log.DebugFormat("Fetching Twitter messages for organization {0}, subscription: {1}", organization.Name, constraints);
                    try {
                        var fetchedMessages = twitterClient.GetNewPublicMessages(subscription, _messageExpiration);
                        messages.AddRange(fetchedMessages);
                        _log.DebugFormat("Recieved {0} messages for organization {1}", fetchedMessages.Count(), organization.Name);
                    } catch(Exception e) {
                        _log.WarnFormat("Could not receive messages for organization {0}, exception: {1}", organization.Name, e.Message);    
                    }
                    _data.Subscriptions.Save(subscription);
                }
            }

            // queue private and direct messages
            foreach (var connection in connections.Where(connection => connection.Active)) {
                _log.DebugFormat(
                    "Fetching {0} messages for organization {0}, identity: {2}", connection.Source, connection.Identity.Id, organization.Name
                );
                try {
                    DateTime lastSearch;
                    var client = NewClient(connection);
                    var fetchedMessages = client.GetNewMessages(_messageExpiration, out lastSearch);
                    connection.LastSearch = lastSearch;
                    messages.AddRange(fetchedMessages);
                    _log.DebugFormat("Received {0} messages for organization {1}", fetchedMessages.Count(), organization.Name);
                } catch(Exception e) {
                    _log.WarnFormat("Could not receive messages for organization {0}, exception: {1}", organization.Name, e.Message);    
                }
                _data.Connections.Save(connection);
            }
            
            var span = DateTime.UtcNow.Subtract(_messageExpiration);
            var stream = _data.GetMessageStream(organization);
            _log.DebugFormat("Queueing {0} messages for organization {1}", messages.Count, organization.Name);
            foreach(var message in messages.Where(message => message.Timestamp >= span)) {
                if(stream.Get(message.Source, message.SourceMessageId) != null) {

                    // message is already in openstream
                    continue;
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
