using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Connections.Email;
using Usivity.Connections.Twitter;
using Usivity.Core.Services.Logic;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Util.Json;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity Core API Service", "", SID = new[] { "sid://usivity.com/2011/01/core" })]
    public partial class CoreService : DreamService {

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        class UsivityFeatureAccessAttribute : Attribute {

            //--- Properties ---
            public User.UserRole Role { get; private set; }

            //--- Constructors ---
            public UsivityFeatureAccessAttribute(User.UserRole role) {
                Role = role; 
            }
        }
        
        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Class Methods ---
        private static T Resolve<T>(DreamContext context) {
            return context.Container.Resolve<T>();
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

        //--- Fields ---
        private Plug _openStreamQueue;
        private TwitterConnectionFactory _twitterConnectionFactory;
        private EmailConnectionFactory _emailConnectionFactory;
        private readonly IDictionary<string, User.UserRole> _features = new Dictionary<string, User.UserRole>();

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

            _log.Debug("Populating internal features access collection");
            var type = GetType(); 
            foreach(var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)) {
                var access = (UsivityFeatureAccessAttribute)Attribute
                    .GetCustomAttribute(method, typeof(UsivityFeatureAccessAttribute));
                if(access == null) {
                    continue;
                }
                var key = string.Format("{0}!{1}", type.FullName, method.Name);
                _features[key] = access.Role;
            }

            _log.Debug("Setting Master API Key");
            MasterApiKey = config["apikey"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(MasterApiKey)) {
                throw new DreamInternalErrorException("Core apikey has not been set in startup config");
            }

            _log.Debug("Initializing source network connection factories");
            _twitterConnectionFactory = new TwitterConnectionFactory(config["sources/twitter"]);
            _emailConnectionFactory = new EmailConnectionFactory(config["sources/email"]);

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
            var data = UsivityDataCatalog.NewUsivityDataCatalog(config["mongodb/connection"].AsText);

            _log.Debug("Registering data session dependency");
            if(!container.IsRegistered<IUsivityDataCatalog>()) {
                builder.RegisterInstance(data).As<IUsivityDataCatalog>().SingleInstance();
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
                builder.RegisterType<Logic.Connections>().As<IConnections>().RequestScoped();
            }
        }

        protected override DreamAccess DetermineAccess(DreamContext context, string key) {
            var featureAccessRole = User.UserRole.None;
            if(UsivityContext.CurrentOrNull == null ||
                !_features.TryGetValue(context.Feature.MainStage.Name, out featureAccessRole)) {

                    // use dream keys to determine feature access if usivity access not set
                    return base.DetermineAccess(context, key);    
            }
            UsivityContext.Current.Role = Resolve<IUsers>(context).GetCurrentRole();
            return UsivityContext.Current.Role >= featureAccessRole ? DreamAccess.Internal : DreamAccess.Public;
        }

        protected Yield PrologueContext(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var auth = Resolve<IUsivityAuth>(context);
            var authToken = auth.GetAuthToken(request);
            var user = auth.GetUser(authToken);
            var usivityContext = new UsivityContext {
                User = user,
                Role = User.UserRole.None,
                ApiUri = Self.Uri.AsPublicUri()
            };
            DreamContext.Current.SetState(usivityContext);
            response.Return(request);
            yield break;
        }
    }
}
