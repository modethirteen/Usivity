using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using MindTouch.OAuth;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Connections.Twitter {

    public class TwitterClient : TwitterConnectionBase, IConnection {

        //--- Constants ---
#if debug
        private const string OAUTH_REQUEST_TOKEN_URI = "http://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "http://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "http://api.twitter.com/oauth/authorize";
#else
        private const string OAUTH_REQUEST_TOKEN_URI = "http://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "http://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "http://api.twitter.com/oauth/authorize";
#endif
        //--- Properties ---
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public Source Source { get; private set; }
        public Identity Identity { get; private set; }
        public bool Active {
            get { return Identity != null; }
        }

        //--- Fields ---
        private readonly IOAuthHandler _oauth;
        private IOAuthAccessToken _oauthAccess;
        private IOAuthRequestToken _oauthRequest;

        //--- Constructors ---
        public TwitterClient(IGuidGenerator guidGenerator, IOrganization organization, XDoc config) {
            Id = guidGenerator.GenerateNewObjectId();
            OrganizationId = organization.Id;
            Source = Source.Twitter;

            var oauthConfig = new OAuthHandlerConfig {
                ConsumerKey = config["oauth/consumer.key"].Contents,
                ConsumerSecret = config["oauth/consumer.secret"].Contents,
                AuthorizeUri = new XUri(OAUTH_AUTHORIZE_URI),
                AccessTokenUri = new XUri(OAUTH_ACCESS_TOKEN_URI),
                RequestTokenUri = new XUri(OAUTH_REQUEST_TOKEN_URI)
            };
            _oauth = new OAuthHandler(oauthConfig);
            
            // generate oauth request token
            XUri callback;
            XUri.TryParse(config["uri.ui"].Contents, out callback);
            if(callback != null) {
                callback = callback.With("connection", Id);
            }
            var response = _oauth.GetRequestTokenResponse(callback);
            _oauthRequest = _oauth.GetRequestToken(response);
        }

        //--- Methods ---
        public IEnumerable<IMessage> GetMessages(IGuidGenerator guidGenerator, IDateTime dateTime, TimeSpan? expiration = null) {
            return new List<IMessage>();
        }

        public IMessage PostReplyMessage(IGuidGenerator guidGenerator, IDateTime dateTime, IMessage message, User user, string reply) {
            if(Identity == null) {
                throw new Exception("Identity is not set");
            }
            reply = string.Format("@{0} {1}", message.Author.Name, reply);

            //TODO: remove temporary test twitter reply logic
            /*var response = Plug.New(API_URI).At("statuses", "update.xml")
                .WithPreHandler(_oauth.AuthorizationHeader)
                .With("status", reply)
                .With("in_reply_to_status_id", message.SourceMessageId)
                .With("include_entities", "true")
                .With("wrap_links", "true")
                .With("trim_user", "true")
                .Post();
            var result = response.ToDocument();*/

            var result = new XDoc("result")
                .Elem("id_str", "FOO" + guidGenerator.GenerateNewObjectId())
                .Elem("text", reply)
                .Elem("from_user_id_str", Identity.Id)
                .Elem("from_user", Identity.Name)
                .Elem("in_reply_to_status_id_str", message.SourceMessageId)
                .Elem("profile_image_url", Identity.Avatar)
                .Elem("created_at", DateTime.UtcNow);
            var replyMessage = GetMessage(guidGenerator, dateTime, result);
            replyMessage.SetParent(message);
            replyMessage.UserId = user.Id;
            return replyMessage;
        }

        public XDoc ToDocument() {
            var doc = new XDoc("connection")
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .Elem("type", "oauth");
            if(Active) {
                doc.Start("identity")
                    .Attr("id", Identity.Id)
                    .Elem("name", Identity.Name)
                    .Elem("uri.avatar", Identity.Avatar)
                .End()
                .Elem("active", true);
            }
            else {
                doc.Elem("uri.authorize", _oauthRequest.AuthorizeUri.ToString())
                    .Elem("active", false);
            }
            return doc; 
        }

        public void Update(XDoc config) {
            if(_oauth == null) {
                var msg = DreamMessage.BadRequest("Connection is not configured for authentication");
                throw new DreamAbortException(msg);
            }
            if(config["oauth/token"].AsText != _oauthRequest.Token) {
                var msg = DreamMessage
                    .BadRequest("Connection update document OAuth request token does not match this connection's OAuth request token");
                throw new DreamAbortException(msg);
            }
            _oauthRequest.Verifier = config["oauth/verifier"].AsText;
            var response = _oauth.GetAccessTokenResponse(_oauthRequest);
            var pairs = XUri.ParseParamsAsPairs(response.ToText())
                .ToDictionary(p => p.Key, p => p.Value);
            _oauth.AccessToken = new OAuthAccessToken(pairs["oauth_token"], pairs["oauth_token_secret"]);

            // construct identity from twitter user lookup
            Identity = GetIdentityByUserId(pairs["user_id"]);

            // dispose request token
            _oauthRequest = null;
        }
    }
}
