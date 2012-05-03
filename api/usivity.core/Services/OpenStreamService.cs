using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Connections.Twitter;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity OpenStream Queue API Service", "", SID = new[] { "sid://usivity.com/2012/04/openstream" })]
    public class OpenStreamService : DreamService {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private IUsivityDataCatalog _data;
        private TwitterPublicConnection _publicTwitterConnection;
        private long _messageCount;

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

            var dbConnection = config["mongodb/connection"].AsText;
            if(string.IsNullOrEmpty(dbConnection)) {
                throw new DreamInternalErrorException("Database connection string required");
            }
            _data = UsivityDataCatalog.NewUsivityDataCatalog(dbConnection);

            _log.Debug("Initializing source connections");
            _publicTwitterConnection = TwitterPublicConnection.GetInstance();

            _log.Debug("Starting open stream queue");
            TaskTimerFactory.Current.New(TimeSpan.Zero, QueueMessages, null, TaskEnv.None);
            TaskTimerFactory.Current.New(TimeSpan.Zero, UpdateMessagesCount, null, TaskEnv.None);
            result.Return();
        }

        private void UpdateMessagesCount(TaskTimer tt) {
            var organizations = _data.Organizations.Get();
            _messageCount = organizations
                .Aggregate<Organization, long>(0, (current, organization) => current + _data.GetMessageStream(organization).GetCount());
            tt.Change(TimeSpan.FromMinutes(1), TaskEnv.None);
        }

        private void QueueMessages(TaskTimer tt) {
            foreach(var organization in _data.Organizations.Get()) {
                _data.GetMessageStream(organization).RemoveExpired();
                Coroutine.Invoke(QueueOrganizationMessages, organization, new Result());    
            }
            tt.Change(TimeSpan.FromMinutes(1), TaskEnv.None);
        }

        private Yield QueueOrganizationMessages(Organization organization, Result result) {
            var messages = new List<Message>();
            var connections = _data.Connections.Get(organization);
            foreach(var subscription in _data.Subscriptions.Get(organization)) {

                // if any organization twitter connections exist get public messages);
                if(connections.Where(connection => connection.Source == Source.Twitter).Count() > 0) {
                    if(subscription.GetSourceUri(Source.Twitter) == null) {
                        _publicTwitterConnection.SetNewSubscriptionUri(subscription);
                    }
                    var constraints = string.Join(",", subscription.Constraints.ToArray());
                    _log.DebugFormat("{0}: Fetching Twitter messages for subscription \"{1}\"", organization.Name, constraints);
                    var fetchedMessages = _publicTwitterConnection.GetMessages(subscription);
                    messages.AddRange(fetchedMessages);
                    _log.DebugFormat("{0}: Recieved {1} messages", organization.Name, fetchedMessages.Count());
                    _data.Subscriptions.Save(subscription);
                }
            }

            // queue private and direct messages
            foreach (var connection in connections.Where(connection => connection.Active)) {
                _log.DebugFormat(
                    "{0}: Fetching {1} messages for {2}", organization.Name, connection.Source, connection.Identity.Id
                );
                var fetchedMessages = connection.GetMessages();
                messages.AddRange(fetchedMessages);
                _data.Connections.Save(connection);
                _log.DebugFormat("{0}: Received {1} messages", organization.Name, fetchedMessages.Count());
            }
            
            var span = DateTime.UtcNow.Subtract(TimeSpan.FromDays(4));
            var stream = _data.GetMessageStream(organization);
            _log.DebugFormat("{0}: Queueing {1} messages", organization.Name, messages.Count);
            foreach(var message in messages.Where(message => message.Timestamp >= span)) {
                if(stream.Get(message.Source, message.SourceMessageId) != null) {

                    // message is already in openstream
                    continue;
                }
                stream.Queue(message);
            }
            _log.DebugFormat("{0}: Finished queueing messages", organization.Name);
            result.Return();
            yield break;
        }
    }
}
