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
using Usivity.Util.Json;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity Core API Service", "",
        SID = new[] { "sid://usivity.com/2011/01/core" }
    )]
    public partial class CoreService : DreamService {
        
        //--- Constants ---
        internal const uint MINUTES_TO_REFRESH = 15;

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private UsivityAuth _auth;
        private User _anonymous;
        private double _refresh;
        private UsivityDataSession _data;
        private int _authExpiration;

        private TwitterConnectionFactory _twitterConnectionFactory;
        private TwitterPublicConnection _publicTwitterConnection;

        private XUri _usersUri;
        private XUri _messagesUri;
        private XUri _connectionsUri;
        private XUri _contactsUri;
        private XUri _organizationsUri;
        private XUri _subscriptionsUri;

        //--- Properties ---
        public string MasterApiKey { get; private set; }

        //--- Methods ---
        public override DreamFeatureStage[] Prologues {
            get {
                return new[] { new DreamFeatureStage("set-context", PrologueContext, DreamAccess.Public) };
            }
        }

        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            _log.Debug("Setting Master API Key");
            MasterApiKey = config["apikey"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(MasterApiKey)) {
                throw new DreamInternalErrorException("Core apikey has not been set in startup config");
            }

            _log.Debug("Initializing current data session");
            var dataSessionConfig = new XDoc("config")
                .Elem("connection", config["mongodb/connection"].AsText);
            UsivityDataSession.Initialize(dataSessionConfig);
            _data = UsivityDataSession.CurrentSession;

            var securitySalt = config["security/salt"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(securitySalt)) {
                throw new DreamInternalErrorException("Security salt string has not been configured");
            }
            var securityCookieUri = config["security/uri.cookie"].AsUri ?? Self.Uri.AsPublicUri();
            _auth = UsivityAuth.Factory(securitySalt, securityCookieUri, _data);
            _authExpiration = config["security/expiration"].AsInt ?? 561600;

            // setup anonymous user
            _anonymous = _data.Users.GetAnonymous();
            if(_anonymous == null) {
                throw new DreamInternalErrorException("Anonymous user has not been configured");
            }

            //TODO: clean up api uris
            var apiUri = Self.Uri.AsPublicUri();
            _usersUri = apiUri.At("users");
            _messagesUri = apiUri.At("messages");
            _connectionsUri = apiUri.At("connections");
            _contactsUri = apiUri.At("contacts");
            _organizationsUri = apiUri.At("organizations");
            _subscriptionsUri = apiUri.At("subscriptions");

            _twitterConnectionFactory = new TwitterConnectionFactory(config["sources/twitter"]);
            _publicTwitterConnection = TwitterPublicConnection.GetInstance();

            _refresh = config["refresh"].AsDouble ?? MINUTES_TO_REFRESH;
            TaskTimerFactory.Current.New(TimeSpan.Zero, QueueMessages, null, TaskEnv.None);
            result.Return();
        }

        protected override DreamAccess DetermineAccess(DreamContext context, string key) {
            var usivityContext = UsivityContext.CurrentOrNull;
            if(usivityContext != null) {
                var role = usivityContext.User.GetOrganizationRole(usivityContext.Organization);
                switch(role) {
                    case User.UserRole.Owner:
                    case User.UserRole.Admin:
                        return DreamAccess.Internal;
                    case User.UserRole.Member:
                        return DreamAccess.Private;
                }
            }
            return base.DetermineAccess(context, key);
        }

        protected Yield PrologueContext(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var authToken = _auth.GetAuthToken(request);
            var user = _auth.GetAuthenticatedUser(authToken) ?? _anonymous;
            var organization = user != null
                ? _data.Organizations.Get(user.CurrentOrganization)
                : Organization.NewMockOrganization();

            var usivityContext = new UsivityContext {
                User = user,
                Organization = organization
            };
            DreamContext.Current.SetState(usivityContext);
            response.Return(request);
            yield break;
        }

        private static XDoc GetRequestXml(DreamMessage request) {
            XDoc doc;
            try {
                var json = new JDoc(request.ToText());
                doc = json.ToDocument();   
            }
            catch {
                try {
                    doc = XDocFactory.From(request.ToText(), MimeType.TEXT_XML);
                    if(doc.IsEmpty) {
                        throw;
                    }
                }
                catch {
                    throw new DreamBadRequestException("Request format must be valid XML or JSON");
                }
            }
            return doc;
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

        private void SetMessageReceipient(Message message) {
            

            //User GetMessageReceipient(Message message);
        }
    }
}
