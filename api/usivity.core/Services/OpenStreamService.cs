using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Data.Connections;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity OpenStream Queue API Service", "",
        SID = new[] { "sid://usivity.com/2012/04/openstream" }
    )]
    public class OpenStreamService : DreamService {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private IUsivityDataSession _data;
        private TwitterPublicConnection _publicTwitterConnection;

        //--- Features ---
        [DreamFeature("GET:status", "Get openstream queue service status")]
        protected Yield GetStatus(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        //--- Methods ---
        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            var dbConnection = config["mongodb/connection"].AsText;
            if(string.IsNullOrEmpty(dbConnection)) {
                throw new DreamInternalErrorException("Database connection string required");
            }
            _data = UsivityDataSession.NewUsivityDataSession(dbConnection);

            _log.Debug("Initializing source connections");
            _publicTwitterConnection = TwitterPublicConnection.GetInstance();

            _log.Debug("Starting open stream queue");
            TaskTimerFactory.Current.New(TimeSpan.Zero, QueueMessages, null, TaskEnv.None);
            result.Return();
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
            foreach(var subscription in _data.Subscriptions.Get(organization)) {

                // if any organization twitter connections exist get public messages
                if(organization.GetConnectionsBySource(SourceType.Twitter).Count() <= 0) {
                    if(subscription.GetSourceUri(SourceType.Twitter) == null) {
                        _publicTwitterConnection.SetNewSubscriptionUri(subscription);
                    }
                    messages.AddRange(_publicTwitterConnection.GetMessages(subscription));
                    _data.Subscriptions.Save(subscription);
                }
            }
            
            var span = DateTime.UtcNow.Subtract(TimeSpan.FromDays(4));
            foreach(var message in messages.Where(message => message.Timestamp >= span)) {
                _data.GetMessageStream(organization).Queue(message);
            }
            result.Return();
            yield break;
        }
    }
}
