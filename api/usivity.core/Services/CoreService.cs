using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;
using Usivity.Core.Libraries;
using Usivity.Core.Libraries.Json;
using Usivity.Data;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Sources;
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity Core API Service", "",
        SID = new[] { "sid://usivity.com/2011/01/core" }
    )]
    public partial class CoreService : DreamService {
        
        //--- Constants ---
        internal const uint MINUTES_TO_REFRESH = 15;

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Class Properties ---
        public static Dictionary<string, OAuthRequestToken> ActiveOAuthRequestTokens { get; set; }

        //--- Fields ---
        private UsivityAuth _auth;
        private User _anonymous;
        private double _refresh;
        private UsivityDataSession _data;
        private IList<ISource> _sources;

        private XUri _usersUri;
        private XUri _messagesUri;
        private XUri _sourcesUri;
        private XUri _contactsUri;
        private XUri _organizationsUri;

        //--- Properties ---
        public string MasterApiKey { get; private set; }

        //--- Methods ---
        public override DreamFeatureStage[] Prologues {
            get {
                return new[] { 
                    new DreamFeatureStage("set-context", PrologueContext, DreamAccess.Public)
                };
            }
        }

        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            _log.Debug("Setting Master API Key");
            MasterApiKey = config["apikey"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(MasterApiKey)) {
                throw new DreamException("Core apikey has not been set in startup config");
            }

            _log.Debug("Initializing current data session");
            var dataSessionConfig = new XDoc("config")
                .Elem("host", config["mongodb/host"].AsText)
                .Elem("database", config["mongodb/database"].AsText);
            UsivityDataSession.Initialize(dataSessionConfig);
            _data = UsivityDataSession.CurrentSession;

            var securitySalt = config["security/salt"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(securitySalt)) {
                throw new DreamInternalErrorException("Security salt string has not been configured");
            }
            _auth = UsivityAuth.Factory(securitySalt);

            // setup sources
            _sources = new List<ISource> {
                new TwitterSource(config["sources/twitter"])
            };
            foreach(var source in _sources) {
                var subscriptions = _data.GetSubscriptions(source.Id);
                foreach(var subscription in subscriptions) {
                    source.Subscriptions.Add(subscription);
                }               
            }

            // setup anonymous user
            _anonymous = _data.GetAnonymousUser()
                ?? new User(User.ANONYMOUS_USER) { Role = User.UserRoles.None };

            var apiUri = Self.Uri.AsPublicUri();
            _usersUri = apiUri.At("users");
            _messagesUri = apiUri.At("messages");
            _sourcesUri = apiUri.At("sources");
            _contactsUri = apiUri.At("contacts");
            _organizationsUri = apiUri.At("organizations");

            ActiveOAuthRequestTokens = new Dictionary<string, OAuthRequestToken>();
            _refresh = config["refresh"].AsDouble ?? MINUTES_TO_REFRESH;
            TaskTimerFactory.Current.New(TimeSpan.Zero, QueueMessages, null, TaskEnv.None);
            TaskTimerFactory.Current.New(TimeSpan.Zero, CleanUpOAuthRequestTokens, null, TaskEnv.None);
            result.Return();
        }

        protected override DreamAccess DetermineAccess(DreamContext context, string key) {
            var usivityContext = UsivityContext.CurrentOrNull;
            if(usivityContext != null) {
                switch(usivityContext.User.Role) {
                    case User.UserRoles.Owner:
                    case User.UserRoles.Admin:
                        return DreamAccess.Internal;
                    case User.UserRoles.Member:
                        return DreamAccess.Private;
                }
            }
            return base.DetermineAccess(context, key);
        }

        protected Yield PrologueContext(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var user = _auth.GetAuthenticatedUser(request);
            if(request.Headers.Authenticate != null) {
                string username, password;
                HttpUtil.GetAuthentication(context.Uri.ToUri(), request.Headers, out username, out password);
                user = _data.GetAuthenticatedUser(username, password);
            }
            if(user == null) {
                user = _anonymous;
            }

            //TODO: dynamic selection of team
            var organization = _data.GetOrganization("1");

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
            _data.RemoveExpiredOpenStreamMessages();

            // get messages from sources
            var messages = new List<Message>();
            foreach(var source in _sources) {
                messages.AddRange(source.GetMessages(Message.MessageStreams.Open));
                foreach(var subscription in source.Subscriptions) {
                    _data.SaveSubscription(subscription);
                }
            }

            // queue messages
            var span = DateTime.UtcNow.Subtract(TimeSpan.FromDays(4));
            var organizations = _data.GetOrganizations();
            foreach (var message in messages.Where(message => message.Timestamp >= span)) {
                foreach(var organization in organizations) {
                    _data.QueueMessage(organization, message);
                }
            }
            tt.Change(TimeSpan.FromMinutes(1), TaskEnv.None);
        }

        private void CleanUpOAuthRequestTokens(TaskTimer tt) {
            var expiredTokenKeys = new List<string>();
            foreach(var requestToken in ActiveOAuthRequestTokens) {
                TimeSpan ts = DateTime.UtcNow - requestToken.Value.Created;
                if(ts.Hours >= 1) {
                    expiredTokenKeys.Add(requestToken.Key);
                }
            }
            foreach(var key in expiredTokenKeys) {
                ActiveOAuthRequestTokens.Remove(key);
            }
            tt.Change(TimeSpan.FromHours(1), TaskEnv.None);
        }
    }
}
