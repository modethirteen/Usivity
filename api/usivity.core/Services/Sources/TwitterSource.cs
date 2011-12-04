using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Core.Libraries;
using Usivity.Core.Libraries.Json;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Sources {

    class TwitterSource : ISource {

        //--- Constants ---
        private const string SEARCH_URI = "http://search.twitter.com/search.json";

#if debug
        private const string OAUTH_REQUEST_TOKEN_URI = "http://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "http://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "http://api.twitter.com/oauth/authorize";
        private const string API_URI = "http://api.twitter.com/1";
#else
        private const string OAUTH_REQUEST_TOKEN_URI = "https://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "https://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "https://api.twitter.com/oauth/authorize";
        private const string API_URI = "https://api.twitter.com/1";
#endif

        //--- Class Properties ---
        private static readonly IDictionary<Subscription.SubscriptionLanguage, string> SubscriptionLanguagesMap = 
            new Dictionary<Subscription.SubscriptionLanguage, string>() {
                { Subscription.SubscriptionLanguage.English, "en" }
        };

        //--- Properties ---
        public string Id { get; private set; }
        public IList<Subscription> Subscriptions { get; set; }
        
        //--- Fields ---
        private readonly OAuth _oauth;
        private readonly Plug _api;

        //--- Constructors ---
        public TwitterSource(XDoc config) {
            Id = "twitter";
            Subscriptions = new List<Subscription>();
            var oauthConfig = new XDoc("config")
                .Elem("consumer.key", config["oauth/consumer.key"].Contents)
                .Elem("consumer.secret", config["oauth/consumer.secret"].Contents)
                .Elem("uri.authorize", OAUTH_AUTHORIZE_URI)
                .Elem("uri.token.request", OAUTH_REQUEST_TOKEN_URI)
                .Elem("uri.token.access", OAUTH_ACCESS_TOKEN_URI)
                .Elem("uri.callback", config["oauth/uri.callback"].Contents);
            _oauth = new OAuth(oauthConfig);
            _api = Plug.New(API_URI);
        }

        //--- Methods ---
        public XDoc GetConnectionXml(User user) {
            var doc = new XDoc("connection").Elem("type", "oauth");
            var isActive = true;
            if(user.GetConnection("twitter") == null) {
                isActive = false;
                var requestToken = CoreService.ActiveOAuthRequestTokens.TryGetValue(user.Id, null);
                if(requestToken == null || requestToken.Created > DateTime.UtcNow.AddHours(1)) {
                    var requestTokenString = _oauth.GetRequestToken().ToText();
                    var pairs = XUri.ParseParamsAsPairs(requestTokenString)
                        .ToDictionary(p => p.Key, p => p.Value);

                    var oauthToken = pairs["oauth_token"];
                    var uri = new XUri(OAUTH_AUTHORIZE_URI).With("oauth_token", oauthToken);

                    requestToken = new OAuthRequestToken(uri, oauthToken, pairs["oauth_token_secret"]);
                    CoreService.ActiveOAuthRequestTokens[user.Id] = requestToken;
                }
                doc.Elem("uri.authorize", requestToken.AuthorizeUri);
            }
            doc.Elem("active", isActive ? "true" : "false");
            doc.EndAll();
            return doc;
        }

        public IList<Message> GetMessages(Message.MessageStreams stream) {
            var messages = new List<Message>();
            foreach (var subscription in Subscriptions) {
                if(!subscription.Active) {
                    continue;
                }
                DreamMessage msg = Plug.New(subscription.Uri).Get();
                if(!msg.IsSuccessful) {
                    continue;
                }
                var response = new JDoc(msg.ToText()).ToDocument("response");
                var nextPage = response["next_page"].Contents;
                var query = !string.IsNullOrEmpty(nextPage) ? nextPage : response["refresh_url"].Contents;
                subscription.Uri = new XUri(SEARCH_URI + query);

                var results = response["results"];
                foreach(var result in results) {
                    messages.Add(GetMessage(result, stream));
                }
            }
            return messages;
        }

        public IConnection GetNewConnection(XDoc doc) {
            var oauth = doc["oauth"] ?? XDoc.Empty;
            if(oauth.IsEmpty) {
                throw new DreamBadRequestException("The requested source network \"twitter\" requires a valid OAuth connection requst");
            }
            var user = doc["userid"].Contents ?? string.Empty;

            var requestToken = CoreService.ActiveOAuthRequestTokens.TryGetValue(user, null);
            if(requestToken == null || oauth["token"].Contents != requestToken.OAuthToken) {
                throw new DreamBadRequestException("You do not have an active OAuth request token or your OAuth request token has expired");
            }
            CoreService.ActiveOAuthRequestTokens.Remove(user);

            requestToken.OAuthVerifier = oauth["verifier"].Contents;
            var accessTokenString = _oauth.GetAccessToken(requestToken).ToText();
            var pairs = XUri.ParseParamsAsPairs(accessTokenString)
                .ToDictionary(p => p.Key, p => p.Value);

            var connection = new OAuthConnection(pairs["oauth_token"], pairs["oauth_token_secret"]) {
                Identity = new SourceIdentity() {
                    Id = pairs["user_id"]
                }
            };
            return connection;
        }

        public Subscription GetNewSubscription(IEnumerable<string> constraints, Subscription.SubscriptionLanguage language) {
            var subscription = new Subscription(Id) {
                Constraints = constraints,
            };
            string lang;
            SubscriptionLanguagesMap.TryGetValue(language, out lang);
            var uri = new XUri(SEARCH_URI).With("q", string.Join(",", constraints.ToArray()));
            if(!string.IsNullOrEmpty(lang)) {
                uri = uri.With("lang", lang);
                subscription.Language = language;
            }
            subscription.Uri = uri.ToUri();
            return subscription;
        }

        public Message PostMessageReply(Message message, string reply, IConnection connection) {
            reply = string.Format("@{0} {1}", message.Author.Name, reply);

            //TODO: remove temporary test twitter reply logic
            /*var response = _api.At("statuses", "update.xml")
                .WithHeaders(connection.GetHeaders())
                .WithPreHandler(_oauth.AuthorizationHeader)
                .With("status", reply)
                .With("in_reply_to_status_id", message.Id)
                .With("include_entities", "true")
                .With("wrap_links", "true")
                .With("trim_user", "true")
                .Post();
            var result = response.ToDocument();*/

            var result = new XDoc("result")
                .Elem("id_str", "foo_" + DateTime.UtcNow.ToShortTimeString())
                .Elem("text", reply)
                .Elem("from_user_id_str", connection.Identity.Id)
                .Elem("from_user", "foo")
                .Elem("profile_image_url",
                      "http://a3.twimg.com/profile_images/1408706495/at-twitter_bigger_reasonably_small.png")
                .Elem("created_at", DateTime.UtcNow);

            var replyMessage = GetMessage(result, Message.MessageStreams.User);
            replyMessage.SetParent(message);
            return replyMessage;
        }

        private Message GetMessage(XDoc result, Message.MessageStreams stream) {
            return new Message(stream) {
                Source = Id,
                SourceMessageId = result["id_str"].AsText,
                Body = result["text"].AsText,
                Author = new SourceIdentity {
                    Id = result["from_user_id_str"].AsText,
                    Name = result["from_user"].AsText,
                    Avatar = result["profile_image_url"].AsUri
                },
                Timestamp = DateTime.Parse(result["created_at"].AsText)
            };
        }
    }
}
