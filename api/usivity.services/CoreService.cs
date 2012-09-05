using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.OAuth;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Services.Clients;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.OAuth;
using Usivity.Services.Clients.Twitter;
using Usivity.Util;
using Usivity.Util.Json;

namespace Usivity.Services {
    using Core;
    using Core.Logic;
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity Core API Service", "", SID = new[] { "sid://usivity.com/2011/01/core" })]
    public class CoreService : DreamService {

        //--- Constants ---
        private const int DEFAULT_MESSAGES_LIMIT = 100;
        private const int DEFAULT_MESSAGES_OFFSET = 0;
        private const int DEFAULT_MESSAGES_START = 3600000;

        //--- Types ---
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
            catch(Exception e) {
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
        private readonly IDictionary<string, User.UserRole> _features = new Dictionary<string, User.UserRole>();

        //--- Properties ---
        public string MasterApiKey { get; private set; }

        //--- Features ---
        #region Connections
        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:connections", "Get all source connections")]
        internal Yield GetConnections(DreamContext context, IConnections connections, Result<DreamMessage> response) {
            var doc = connections.GetConnectionsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:connections/{connectionid}", "Get source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield GetConnection(DreamContext context, IConnections connections, Result<DreamMessage> response) {
            var connection = connections.GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:connections/twitter/token", "Get twitter new oauth request token")]
        internal Yield GetTwitterConnectionRequest(DreamContext context, IConnections connections, Result<DreamMessage> response) {
            var token = connections.NewOAuthRequestToken(Source.Twitter);
            var doc = connections.GetOAuthRequestTokenXml(token);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }
        
        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("POST:connections/twitter", "Add new twitter source connection")]
        internal Yield PostTwitterConnection(DreamContext context, DreamMessage request, IConnections connections, Result<DreamMessage> response) {
            var auth = GetRequestXml(request);
            if(auth["token"].IsEmpty || auth["verifier"].IsEmpty) {
                response.Return(DreamMessage.BadRequest("Twitter connections require an oauth request token and verifier"));
                yield break;
            }
            var requestToken = connections.GetOAuthRequestToken(Source.Twitter, auth["token"].AsText);
            if(requestToken == null) {
                response.Return(DreamMessage.BadRequest("Requested oauth request token information does not exist"));
                yield break;
            }
            var connection = connections.NewTwitterConnection(requestToken, auth["verifier"].AsText);
            connections.SaveConnection(connection);
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("POST:connections/email", "Add new email source connection")]
        internal Yield PostEmailConnection(DreamContext context, DreamMessage request, IConnections connections, Result<DreamMessage> response) {
            var auth = GetRequestXml(request);
            if(auth["host"].IsEmpty || auth["username"].IsEmpty) {
                response.Return(DreamMessage.BadRequest("Email connections require an imap host and username"));
                yield break;
            }
            var host = auth["host"].AsText;
            var username = auth["username"].AsText;
            var password = auth["password"].AsText;
            var port = auth["port"].AsInt ?? 143;
            var useSsl = auth["ssl"].AsBool ?? false;
            var authMethod = auth["auth.method"].AsText;
            if(!string.IsNullOrEmpty(authMethod)) {
                authMethod = authMethod.ToLowerInvariant();
            }
            var useCramMd5 = false;
            switch(authMethod) {
                case "crammd5":
                    useCramMd5 = true;
                    break;
                case "login":
                case null:
                    break;
                default:
                    response.Return(DreamMessage.BadRequest("\"auth.method\" element is invalid. Allowed values: login, crammd5"));
                    yield break;
            }
            var connection = connections.NewEmailConnection(host, username, password, port, useSsl, useCramMd5);
            connections.SaveConnection(connection);
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("DELETE:connections/{connectionid}", "Delete source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield DeleteConnection(DreamContext context, IConnections connections, Result<DreamMessage> response) {
            var connection = connections.GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            connections.DeleteConnection(connection);
            response.Return(DreamMessage.Ok());
            yield break;
        }
        #endregion

        #region Contacts
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:contacts", "Get all contacts")]
        internal Yield GetContacts(DreamContext context, IContacts contacts, Result<DreamMessage> response) {
            var doc = contacts.GetContactsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:contacts/{contactid}", "Get contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        [DreamFeatureParam("messages", "{tree,flat}?", "Conversation message hiearchy mode (default: tree)")]
        internal Yield GetContact(DreamContext context, IContacts contacts, Result<DreamMessage> response) {
            var contact = contacts.GetContact(context.GetParam<string>("contactid"));
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            var tree = context.GetParam("messages", "tree").ToLowerInvariant() == "tree";
            var messages = Resolve<IMessages>(context);
            var doc = contacts.GetContactVerboseXml(contact).AddAll(messages.GetConversationsXml(contact, tree));
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("POST:contacts", "Create a new contact")]
        internal Yield PostContact(DreamContext context, DreamMessage request, IContacts contacts, Result<DreamMessage> response) {
            var contact = contacts.GetNewContact(GetRequestXml(request));
            contacts.SaveContact(contact);
            var doc = contacts.GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("PUT:contacts/{contactid}", "Update contact information")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        internal Yield UpdateContact(DreamContext context, DreamMessage request, IContacts contacts, Result<DreamMessage> response) {
            var contact = contacts.GetContact(context.GetParam<string>("contactid"));
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            contacts.UpdateContactInformation(contact, GetRequestXml(request));
            contacts.SaveContact(contact);
            var doc = contacts.GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("DELETE:contacts/{contactid}", "Remove contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        internal Yield RemoveContact(DreamContext context, IContacts contacts, Result<DreamMessage> response) {
            var contact = contacts.GetContact(context.GetParam<string>("contactid")); 
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            contacts.RemoveContact(contact);
            response.Return(DreamMessage.Ok());
            yield break;
        }
        #endregion

        #region Messages
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:messages", "Get messages from stream")]
        [DreamFeatureParam("limit", "int?", "Max messages to receive ranging 1 - 100 (default: 100)")]
        [DreamFeatureParam("offset", "int?", "Receive messages starting with offset (default: none)")]
        [DreamFeatureParam("start", "int?", "Milliseconds back in time to stream (default: 3600000)")]
        [DreamFeatureParam("source", "{email,twitter}?", "Filter by source")]
        internal Yield GetMessages(DreamContext context, IMessages messages, Result<DreamMessage> response) {
            var count = context.GetParam("limit", DEFAULT_MESSAGES_LIMIT);
            var offset = context.GetParam("offset", DEFAULT_MESSAGES_OFFSET);
            var start = context.GetParam("start", DEFAULT_MESSAGES_START);
            var sourceFilter = context.GetParam("source", null);
            Source? source = null;
            if(!string.IsNullOrEmpty(sourceFilter)) {
                switch(sourceFilter.ToLowerInvariant()) {
                    case "twitter":
                        source = Source.Twitter;
                        break;
                    case "email":
                        source = Source.Email;
                        break;
                    default:
                        throw new DreamBadRequestException(string.Format("\"{0}\" is not a valid source filter parameter", sourceFilter));
                }       
            }

            // TODO: reconcile datetime reference here and in openstream service
            var dateTime = new DateTimeImpl();
            var startTime = dateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(start));
            var doc = messages.GetMessageStreamXml(startTime, dateTime.UtcNow, count, offset, source); 
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:messages/{messageid}", "Get message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        [DreamFeatureParam("children", "{tree,flat}?", "Child message hiearchy mode (default: tree)")]
        internal Yield GetMessage(DreamContext context, IMessages messages, Result<DreamMessage> response) {
            var message = messages.GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var tree = context.GetParam("children", "tree").ToLowerInvariant() == "tree";
            var doc = messages.GetMessageVerboseXml(message, tree);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("DELETE:messages/{messageid}", "Delete message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        internal Yield DeleteMessage(DreamContext context, IMessages messages, Result<DreamMessage> response) {
            var message = messages.GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            messages.DeleteMessage(message);
            response.Return(DreamMessage.Ok());
            yield break;
        }
     
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("POST:messages/{messageid}", "Post message in reply")]
        [DreamFeatureParam("message", "string", "Message id to reply to")]
        internal Yield PostMessageReply(DreamContext context, DreamMessage request, IMessages messages, Result<DreamMessage> response) {
            var message = messages.GetMessage(context.GetParam<string>("messageid"));          
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var doc = messages.PostReply(message, request.ToText());
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }
        #endregion

        #region Organizations
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:organizations", "Get organizations")]
        internal Yield GetOrganizations(DreamContext context, IOrganizations organizations, Result<DreamMessage> response) {
            var doc = organizations.GetOrganizationsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:organizations/{organizationid}", "Get organization")]
        [DreamFeatureParam("organizationid", "string", "Organization id")]
        internal Yield GetOrganization(DreamContext context, IOrganizations organizations, Result<DreamMessage> response) {
            var organization = organizations.GetOrganization(context.GetParam<string>("organizationid"));
            if(organization == null) {
                response.Return(DreamMessage.NotFound("The requested organization could not be located"));
                yield break;
            }
            var doc = organizations.GetOrganizationXml(organization);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }
        #endregion

        #region Subscriptions
        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:subscriptions", "Get all subscriptions")]
        internal Yield GetSubscriptions(DreamContext context, ISubscriptions subscriptions, Result<DreamMessage> response) {
            var doc = subscriptions.GetSubscriptionsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:subscriptions/{subscriptionid}", "Get a subscription")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        internal Yield GetSubscription(DreamContext context, ISubscriptions subscriptions, Result<DreamMessage> response) {
            var subscription = subscriptions.GetSubscription(context.GetParam<string>("subscriptionid"));
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            var doc = subscriptions.GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("POST:subscriptions", "Add a new subscription")]
        internal Yield PostSubscription(DreamContext context, DreamMessage request, ISubscriptions subscriptions, Result<DreamMessage> response) {
            DreamMessage msg;
            try {
                var subscription = subscriptions.GetNewSubscription(GetRequestXml(request));
                subscriptions.SaveSubscription(subscription);
                msg = DreamMessage.Ok(subscriptions.GetSubscriptionXml(subscription));
            }
            catch(DreamAbortException e) {
                msg = e.Response; 
            }
            response.Return(msg);
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("DELETE:subscriptions/{subscriptionid}", "Delete a subscription")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        internal Yield DeleteSubscription(DreamContext context, ISubscriptions subscriptions, Result<DreamMessage> response) {
            var subscription = subscriptions.GetSubscription(context.GetParam<string>("subscriptionid"));
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            subscriptions.DeleteSubscription(subscription);
            response.Return(DreamMessage.Ok());
            yield break;
        }
        #endregion

        #region Users
        [UsivityFeatureAccess(User.UserRole.None)]
        [DreamFeature("GET:users/authentication", "Get user authentication")]
        [DreamFeatureParam("redirect", "uri?", "Redirect to uri upon authentication")]
        internal Yield GetUserAuthentication(DreamContext context, DreamMessage request, IUsers users, IUsivityAuth auth, Result<DreamMessage> response) {
            User user;
            string authToken;
            if(request.Headers.Authorization != null) {
                string username, password;
                HttpUtil.GetAuthentication(context.Uri.ToUri(), request.Headers, out username, out password);
                password = auth.GetSaltedPassword(password);
                user = users.GetAuthenticatedUser(username, password);
                authToken = auth.GenerateAuthToken(user);
            }
            else {
                authToken = auth.GetAuthToken(request);
                user = auth.GetUser(authToken);
            }
            if(user == null || user.IsAnonymous) {
                response.Return(new DreamMessage(DreamStatus.Unauthorized, new DreamHeaders()));
                yield break;
            }
            var redirect = XUri.TryParse(context.GetParam("redirect", null));
            var setCookie = auth.GetAuthCookie(authToken, Self.Uri.AsPublicUri()).ToSetCookieHeader();
            var authResponse = redirect != null ? DreamMessage.Redirect(redirect) : DreamMessage.Ok(MimeType.TEXT_UTF8, setCookie);
            authResponse.Headers["Set-Cookie"] = setCookie;
            response.Return(authResponse);
            yield break;
        }
        
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:users", "Get all users")]
        internal Yield GetUsers(DreamContext context,  IUsers users, Result<DreamMessage> response) {
            var doc = users.GetUsersXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:users/current", "Get current user")]
        internal Yield GetCurrentUser(DreamContext context, IUsers users, Result<DreamMessage> response) {
            var doc = users.GetCurrentUserXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }
        
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:users/{userid}", "Get user")]
        [DreamFeatureParam("userid", "string", "User id or \"current\"")]
        internal Yield GetUser(DreamContext context, IUsers users, Result<DreamMessage> response) {
            var userId = context.GetParam<string>("userid");
            var currentUser = users.GetCurrentUser();
            var user = (userId != currentUser.Id && !userId.Equals("current")) ? users.GetUser(userId) : currentUser;
            if(user == null) {
                response.Return(DreamMessage.NotFound("The requested user could not be located"));
                yield break;
            }
            var doc = users.GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("POST:users", "Create a new user")]
        internal Yield PostUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var userDoc = GetRequestXml(request);
            var name = userDoc["name"].AsText;
            var email = userDoc["email"].AsText;
            var password = userDoc["password"].AsText;
            if(string.IsNullOrEmpty(name)) {
                response.Return(DreamMessage.BadRequest("Request is missing a value for \"name\""));
                yield break;
            }
            if(name.EqualsInvariantIgnoreCase(User.ANONYMOUS_USER)) {
                response.Return(DreamMessage.BadRequest("\"" + User.ANONYMOUS_USER + "\" is not a valid user name"));
                yield break;
            }
            if(string.IsNullOrEmpty(password)) {
                response.Return(DreamMessage.BadRequest("Request is missing a value for \"password\""));
                yield break;
            }
            if(string.IsNullOrEmpty(email)) {
                response.Return(DreamMessage.BadRequest("Request is missing a value for \"email\""));
                yield break;
            }
            if(!EmailClient.IsValidEmailAddress(email)) {
                response.Return(DreamMessage.BadRequest("Email address is invalid"));
                yield break;
            }
            var users = Resolve<IUsers>(context);
            if(users.UsernameExists(name)) {
                response.Return(DreamMessage.Conflict("The requested user name has already been taken"));
            }
            var user = users.NewUser(name, password, email);
            users.SaveUser(user);
            var doc = users.GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("PUT:users/{userid}", "Update user")]
        [DreamFeatureParam("userid", "string", "User id or \"current\"")]
        internal Yield UpdateUserPassword(DreamContext context, DreamMessage request, IUsivityAuth auth, IUsers users, IOrganizations organizations, Result<DreamMessage> response) {
            var userId = context.GetParam<string>("userid");
            var currentUser = users.GetCurrentUser();
            var currentRole = currentUser.GetOrganizationRole(organizations.CurrentOrganization);
            User user;
            if(userId != currentUser.Id && !userId.Equals("current")) {
                if(currentRole <= User.UserRole.Member) {
                    response.Return(DreamMessage.Forbidden("You can only update your own user information"));
                    yield break;
                }
                user = users.GetUser(userId);
            } else {
                user = currentUser;
            } 
            var userDoc = GetRequestXml(request);
            if(!userDoc["name"].IsEmpty) {
                user.Name = userDoc["name"].AsText;  
            }
            if(!userDoc["email"].IsEmpty) {
                var email = userDoc["email"].AsText;
                if(!EmailClient.IsValidEmailAddress(email)) {
                    response.Return(DreamMessage.BadRequest("Email address is invalid"));
                    yield break;
                }
                user.EmailAddress = email;
            }
            if(!userDoc["password"].IsEmpty) {
                user.Name = auth.GetSaltedPassword(userDoc["password"].AsText);
            }
            users.SaveUser(user);
            response.Return(DreamMessage.Ok(MimeType.TEXT_UTF8, "User has been successfully updated"));
            yield break;
        }
        #endregion
        
        //--- Methods ---
        public override DreamFeatureStage[] Prologues {
            get { return new[] { new DreamFeatureStage("set-context", PrologueContext, DreamAccess.Public) }; }
        }

        public override ExceptionTranslator[] ExceptionTranslators {
            get { return new ExceptionTranslator[] { Translator }; }
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

            _log.Debug("Starting openstream queue service");
            yield return CreateService(
                "openstream",
                "sid://usivity.com/2012/04/openstream",
                new XDoc("config")
                    .Elem("apikey", MasterApiKey)
                    .AddAll(config["aws"])
                    .AddAll(config["openstream"])
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
            if(!container.IsRegistered<IDateTime>()) {
                _log.Debug("Registering current request datetime");
                builder.Register(c => new DateTimeImpl(DreamContext.Current.StartTime)).As<IDateTime>().RequestScoped();
            }

            _log.Debug("Initializing source network clients");
            if(!container.IsRegistered<ITwitterClientFactory>()) {
                builder.Register(c => {
                    var twitterConfig = new OAuthConfig {
                        ConsumerKey = config["sources/twitter/consumer.key"].AsText,
                        ConsumerSecret = config["sources/twitter/consumer.secret"].AsText,
                        NonceFactory = new OAuthNonceFactory(),
                        TimeStampFactory = new OAuthTimeStampFactory()
                    };
                    var guidGenerator = c.Resolve<IGuidGenerator>();
                    var dateTime = c.Resolve<IDateTime>();
                    return new TwitterClientFactory(twitterConfig, guidGenerator, dateTime);
                })
                .As<ITwitterClientFactory>()
                .RequestScoped();
            }
            if(!container.IsRegistered<IEmailClientFactory>()) {
                builder.Register(c => {
                    var emailClientConfig = new SimpleEmailServiceConfig {
                        AwsPublicKey = config["aws/public.key"].AsText,
                        AwsPrivateKey = config["aws/private.key"].AsText
                    };
                    var guidGenerator = c.Resolve<IGuidGenerator>();
                    var dateTime = c.Resolve<IDateTime>();
                    return new EmailClientFactory(emailClientConfig, guidGenerator, dateTime);
                })
                .As<IEmailClientFactory>()
                .RequestScoped();
            }

            _log.Debug("Initializing data catalog connection");
            var data = UsivityDataCatalog.NewUsivityDataCatalog(config["mongodb/connection"].AsText);
            if(!container.IsRegistered<IUsivityDataCatalog>()) {
                _log.Debug("Registering data catalog dependency");
                builder.RegisterInstance(data).As<IUsivityDataCatalog>().SingleInstance();
            }

            _log.Debug("Initializing authorization controls");
            var securitySalt = config["security/salt"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(securitySalt)) {
                throw new DreamInternalErrorException("Security salt string has not been configured");
            }
            var authExpiration = config["security/expiration"].AsInt ?? 561600;
            if(!container.IsRegistered<IUsivityAuth>()) {
                _log.Debug("Registering authorization dependency");
                builder.Register(c => new UsivityAuth(securitySalt, authExpiration, c.Resolve<IDateTime>(), data))
                .As<IUsivityAuth>()
                .RequestScoped();
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

             _log.Debug("Registering utilities");
            if(!container.IsRegistered<IGuidGenerator>()) {
                builder.RegisterType<GuidGenerator>().As<IGuidGenerator>().RequestScoped();
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
            var apiUri = Self.Uri.AsPublicUri();
            var usivityContext = new UsivityContext {
                User = user,
                Role = User.UserRole.None,
                ApiUri = apiUri,

                // Assumes UI is on same domain as API
                UiUri = new XUri(apiUri.WithoutPathQueryFragment())
            };
            DreamContext.Current.SetState(usivityContext);
            response.Return(request);
            yield break;
        }

        private DreamMessage Translator(DreamContext context, Exception e) {
            _log.Error(e);
            if(e is ConnectionResponseException) {
                var connectionResponseException = (ConnectionResponseException)e;
                switch(connectionResponseException.Status) {
                    case DreamStatus.BadRequest:
                        return DreamMessage.BadRequest(e.Message);
                    case DreamStatus.NotFound:
                        return DreamMessage.NotFound(e.Message);
                    default:
                        return new UsivityInternalErrorMessage();
                }
            }
            return new UsivityInternalErrorMessage();
        }
    }
}
