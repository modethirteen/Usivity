using System.Collections.Generic;
using Autofac;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Core.Services.Logic;
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
        
        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Class Methods ---
        private static T Resolve<T>(DreamContext context) {
            return context.Container.Resolve<T>();
        }

        //--- Fields ---
        private Plug _openStreamQueue;
        private TwitterConnectionFactory _twitterConnectionFactory;


        //--- Properties ---
        public string MasterApiKey { get; private set; }

        //--- Methods ---
        public override DreamFeatureStage[] Prologues {
            get {
                return new[] { new DreamFeatureStage("set-context", PrologueContext, DreamAccess.Public) };
            }
        }

        protected override Yield Start(XDoc config, ILifetimeScope container, Result result) {
            yield return Coroutine.Invoke(base.Start, config, container, new Result());

            _log.Debug("Setting Master API Key");
            MasterApiKey = config["apikey"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(MasterApiKey)) {
                throw new DreamInternalErrorException("Core apikey has not been set in startup config");
            }

            _log.Debug("Initializing source network connections");
            _twitterConnectionFactory = new TwitterConnectionFactory(config["sources/twitter"]);
          
            _log.Debug("Starting openstream queue service");
            yield return CreateService(
                "openstream",
                "sid://usivity.com/2012/04/openstream",
                new XDoc("config")
                    .Elem("apikey", MasterApiKey)
                    .AddAll(config["mongodb"])
                    .AddAll(config["sources"]),
                new Result<Plug>()
                );
            _openStreamQueue = Plug.New(Self.Uri).At("openstream");
            result.Return();
        }

        protected override void InitializeLifetimeScope(IRegistrationInspector container, ContainerBuilder builder, XDoc config) {

            _log.Debug("Registering current context container");
            builder.Register(c => UsivityContext.Current).RequestScoped();
            builder.Register(c => c.Resolve<UsivityContext>()).As<ICurrentContext>().RequestScoped();

            _log.Debug("Initializing data session connection");
            var data = UsivityDataSession.NewUsivityDataSession(config["mongodb/connection"].AsText);

            _log.Debug("Registering data session dependency");
            if(!container.IsRegistered<IUsivityDataSession>()) {
                builder.RegisterInstance(data).As<IUsivityDataSession>().SingleInstance();
            }

            _log.Debug("Initializing authorization controls");
            var securitySalt = config["security/salt"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(securitySalt)) {
                throw new DreamInternalErrorException("Security salt string has not been configured");
            }
            var authExpiration = config["security/expiration"].AsInt ?? 561600;
            var auth = new UsivityAuth(securitySalt, authExpiration, data);

            _log.Debug("Registering authorization dependency");
            if(!container.IsRegistered<IUsivityAuth>()) {
                builder.RegisterInstance(auth).As<IUsivityAuth>().SingleInstance();
            }

            _log.Debug("Registering type dependencies");
            if(!container.IsRegistered<IUsers>()) {
                builder.RegisterType<Users>().As<IUsers>().RequestScoped();
            }
            if(!container.IsRegistered<IMessages>()) {
                builder.RegisterType<Messages>().As<IMessages>().RequestScoped();
            }
            if(!container.IsRegistered<IOrganizations>()) {
                builder.RegisterType<Organizations>().As<IOrganizations>().RequestScoped();
            }
            if(!container.IsRegistered<IContacts>()) {
                builder.RegisterType<Contacts>().As<IContacts>().RequestScoped();
            }
            if(!container.IsRegistered<ISubscriptions>()) {
                builder.RegisterType<Subscriptions>().As<ISubscriptions>().RequestScoped();
            }
            if(!container.IsRegistered<IConnections>()) {
                builder.RegisterType<Connections>().As<IConnections>().RequestScoped();
            }
        }

        protected override DreamAccess DetermineAccess(DreamContext context, string key) {
            if(UsivityContext.CurrentOrNull != null ) {
                var role = Resolve<IUsers>(context).GetCurrentRole();
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
            var auth = Resolve<IUsivityAuth>(context);
            var authToken = auth.GetAuthToken(request);
            var user = auth.GetUser(authToken);
            var usivityContext = new UsivityContext {
                User = user,
                ApiUri = Self.Uri.AsPublicUri()
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
    }
}
