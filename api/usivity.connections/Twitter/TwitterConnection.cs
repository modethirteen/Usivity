using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Connections.Twitter {

    public class TwitterConnection : TwitterConnectionBase, IConnection {

        //--- Constants ---
#if debug
        private const string OAUTH_REQUEST_TOKEN_URI = "http://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "http://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "http://api.twitter.com/oauth/authorize";
#else
        private const string OAUTH_REQUEST_TOKEN_URI = "https://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "https://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "https://api.twitter.com/oauth/authorize";
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
        private OAuth _oauth;
        private OAuthRequest _oauthRequest;

        //--- Constructors ---
        public TwitterConnection(XDoc config, Organization organization) {
            Id = GuidGenerator.CreateUnique();
            OrganizationId = organization.Id;
            Source = Source.Twitter;
            var oauthConfig = new XDoc("config")
                .Elem("consumer.key", config["oauth/consumer.key"].Contents)
                .Elem("consumer.secret", config["oauth/consumer.secret"].Contents)
                .Elem("uri.authorize", OAUTH_AUTHORIZE_URI)
                .Elem("uri.token.request", OAUTH_REQUEST_TOKEN_URI)
                .Elem("uri.token.access", OAUTH_ACCESS_TOKEN_URI)
                .Elem("uri.callback", config["oauth/uri.callback"].Contents);
            _oauth = new OAuth(oauthConfig);
            
            // generate oauth request token
            var requestToken = _oauth.GetRequestToken().ToText();
            var pairs = XUri.ParseParamsAsPairs(requestToken)
                .ToDictionary(p => p.Key, p => p.Value);
            var oauthToken = pairs["oauth_token"];
            var uri = new XUri(OAUTH_AUTHORIZE_URI).With("oauth_token", oauthToken);
            _oauthRequest = new OAuthRequest(uri, oauthToken, pairs["oauth_token_secret"]);
        }

        //--- Methods ---
        public IEnumerable<Message> GetMessages() {
            return new List<Message>();
        }

        public Message PostReplyMessage(Message message, User user, string reply) {
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
                .Elem("id_str", "foo_" + DateTime.UtcNow.ToShortTimeString())
                .Elem("text", reply)
                .Elem("from_user_id_str", Identity.Id)
                .Elem("from_user", Identity.Name)
                .Elem("in_reply_to_status_id_str", message.SourceMessageId)
                .Elem("profile_image_url", Identity.Avatar)
                .Elem("created_at", DateTime.UtcNow);
            var replyMessage = GetMessage(result);
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
                var response = DreamMessage.BadRequest("Connection is not configured for authentication");
                throw new DreamAbortException(response);
            }
            if(config["oauth/token"].AsText != _oauthRequest.Token) {
                var response = DreamMessage
                    .BadRequest("Connection update document OAuth request token does not match this connection's OAuth request token");
                throw new DreamAbortException(response);
            }
            _oauthRequest.Verifier = config["oauth/verifier"].AsText;
            var accessTokenText = _oauth.GetAccessToken(_oauthRequest).ToText();
            var pairs = XUri.ParseParamsAsPairs(accessTokenText)
                .ToDictionary(p => p.Key, p => p.Value);
            _oauth.AccessToken = pairs["oauth_token"];
            _oauth.AccessTokenSecret = pairs["oauth_token_secret"];

            // construct identity from twitter user lookup
            Identity = GetIdentityByUserId(pairs["user_id"]);

            // dispose request token
            _oauthRequest = null;
        }
    }
}
