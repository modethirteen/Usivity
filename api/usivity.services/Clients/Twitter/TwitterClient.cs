﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Xml;
using MindTouch.OAuth;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Util;
using Usivity.Util.Json;

namespace Usivity.Services.Clients.Twitter {

    public class TwitterClient : ITwitterClient {

        //--- Constants ---
        private const string SEARCH_URI = "http://search.twitter.com/search.json";
        private const uint MAX_RESULTS = 100;
#if DEBUG
        private const string API_URI = "http://api.twitter.com/1";
        private const string OAUTH_REQUEST_TOKEN_URI = "http://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "http://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "http://api.twitter.com/oauth/authorize";
#else
        private const string API_URI = "https://api.twitter.com/1";
        private const string OAUTH_REQUEST_TOKEN_URI = "https://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "https://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "https://api.twitter.com/oauth/authorize";
#endif
        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();
        private static readonly IDictionary<Subscription.SubscriptionLanguage, string> _subscriptionLanguagesMap = 
            new Dictionary<Subscription.SubscriptionLanguage, string> {
                { Subscription.SubscriptionLanguage.English, "en" }
        };

        //--- Class Methods ---
        public static Identity GetIdentityByScreenName(string screenName) {
             var msg = Plug.New(API_URI)
                .At("users", "show.xml")
                .With("screen_name", screenName)
                .Get();
            return ParseUserLookupResult(msg);
        }

        private static XUri GetNewSubscriptionUri(Subscription subscription) {
            string lang;
            _subscriptionLanguagesMap.TryGetValue(subscription.Language, out lang);
            var uri = new XUri(SEARCH_URI)
                .With("rpp", MAX_RESULTS.ToString())
                .With("q", string.Join(",", subscription.Constraints.ToArray()));
            if(!string.IsNullOrEmpty(lang)) {
                uri = uri.With("lang", lang);
            }
            return uri;
        }

        private static Identity GetIdentityByUserId(string userId) {
            var msg = Plug.New(API_URI)
                .At("users", "show.xml")
                .With("user_id", userId)
                .Get();
            return ParseUserLookupResult(msg);
        }

        private static Identity ParseUserLookupResult(DreamMessage result) {
            var userDoc = result.ToDocument();
            if(!userDoc["error"].IsEmpty) {
                return null;
            }
            XUri uri;
            XUri.TryParse(userDoc["profile_image_url"].AsText, out uri);
            return new Identity {
                Id = userDoc["id"].AsText,
                Name = userDoc["screen_name"].AsText,
                Avatar = uri != null ? uri.ToUri() : null
            };
        }

        //--- Fields ---
        private readonly ITwitterConnection _connection;
        private readonly IOAuthHandler _oauth;
        private readonly XUri _oauthCallbackBaseUri;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTime _dateTime;

        //--- Constructors ---
        public TwitterClient(TwitterClientConfig config, ITwitterConnection connection, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _connection = connection;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
            var oauthConfig = new OAuthHandlerConfig {
                ConsumerKey = config.OAuthConsumerKey,
                ConsumerSecret = config.OAuthConsumerSecret,
                AuthorizeUri = new XUri(OAUTH_AUTHORIZE_URI),
                AccessTokenUri = new XUri(OAUTH_ACCESS_TOKEN_URI),
                RequestTokenUri = new XUri(OAUTH_REQUEST_TOKEN_URI),
                NonceFactory = config.OAuthNonceFactory,
                TimeStampFactory = config.OAuthTimeStampFactory
            };
            _oauth = new OAuthHandler(oauthConfig);
            _oauthCallbackBaseUri = config.OAuthCallbackBaseUri;
        }

        //--- Methods ---
        #region IClient implementations
        public IEnumerable<IMessage> GetNewMessages(TimeSpan? expiration, out DateTime lastSearch) {
            throw new NotImplementedException();
        }

        public IMessage PostNewReplyMessage(IUser user, IMessage message, string reply) {
            var plug = Plug.New(API_URI).At("statuses", "update.json")
                .With("status", string.Format("@{0} {1}", message.Author.Name, reply))
                .With("in_reply_to_status_id", message.SourceMessageId)
                .With("include_entities", "true")
                .With("wrap_links", "true")
                .With("trim_user", "true");
            DreamMessage response;
            try {
                response = plug.WithOAuthAuthentication(_oauth, _connection.OAuthAccess).PostAsForm();
            }
            catch(DreamResponseException e) {
                throw new ConnectionResponseException(e.Response.Status, "Error posting message", e);
            }
            var replyMessage = NewFromStatusUpdate(new JDoc(response.ToText()).ToDocument("response"));
            replyMessage.SetParent(message);
            replyMessage.UserId = user.Id;
            return replyMessage;
        }
  
        public Contact NewContact(Identity identity) {
            var contact = new Contact(_guidGenerator);
            contact.SetIdentity(Source.Twitter, identity);
            if(string.IsNullOrEmpty(identity.Id)) {
                return contact;
            }
            try {
                var msg = Plug.New(API_URI)
                    .At("users", "show.xml")
                    .With("user_id", identity.Id)
                    .Get();
                var userDoc = msg.ToDocument();
                if(string.IsNullOrEmpty(contact.FirstName)) {
                    contact.FirstName = userDoc["name"].AsText;
                }
                XUri uri;
                XUri.TryParse(userDoc["profile_image_url"].AsText, out uri);
                if(uri != null && contact.Avatar == null) {
                    contact.Avatar = uri;
                }
                if(string.IsNullOrEmpty(contact.Location)) {
                    contact.Location = userDoc["location"].AsText;
                }
            }
            catch(Exception e) {
                _log.WarnFormat("Cannot populate contact data with Twitter details, exception: {0}", e.Message);        
            }
            return contact;
        }
        #endregion

        #region IPublicClient implementations
        public IEnumerable<IMessage> GetNewPublicMessages(Subscription subscription, TimeSpan? expiration) {
            var messages = new List<IMessage>();
            if(!subscription.Active) {
                return messages;
            }
            var uri = new XUri(subscription.GetSourceUri(Source.Twitter) ?? GetNewSubscriptionUri(subscription));
            var cursor = uri.GetParam("since_id", ulong.MinValue);
            while(true) {
                DreamMessage msg;
                try {
                    msg = Plug.New(uri).Get(); 
                } catch(Exception e) {
                    _log.Warn("Error while fetching twitter messages, exception: " + e.Message);
                    return messages;
                }
                var response = new JDoc(msg.ToText()).ToDocument("response");
                var results = response["results"];
                messages.AddRange(results.Select(result => NewFromSearchResult(result, expiration)));
                var ids = new List<ulong>();
                foreach(var result in results) {
                    var id = ulong.MinValue;
                    ulong.TryParse(result["id_str"].AsText, out id);
                    if(id != ulong.MinValue) {
                        ids.Add(id);
                    }
                }
                if(ids.Count == MAX_RESULTS && cursor > ulong.MinValue) {
                    
                    // move start range to next page
                    var start = ids.Last() - 1;
                    uri = GetNewSubscriptionUri(subscription)
                        .With("max_id", start.ToString())
                        .With("since_id", cursor.ToString());
                }
                else {
                    if(ids.Last() > cursor) {
                        
                        // move end range to highest id ever fetched
                        cursor = ids.First();     
                    }
                    uri = GetNewSubscriptionUri(subscription).With("since_id", cursor.ToString());
                    break;
                }
            }
            subscription.SetSourceUri(Source.Twitter, uri);
            return messages;
        }
        #endregion

        #region ITwitterClient implementation
        public Identity GetIdentity(string screenName) {
            return GetIdentityByScreenName(screenName);
        }

        public OAuthRequestToken NewOAuthRequestToken() {
            XUri callback = null;
            if(_oauthCallbackBaseUri != null) {
                callback = _oauthCallbackBaseUri.With("connection", _connection.Id);    
            }
            var response = _oauth.GetRequestTokenResponse(callback);
            if(!response.IsSuccessful) {
                throw new DreamResponseException(response, "Failed to retrieve oauth request token");
            }
            return _oauth.GetRequestToken(response);
        }

        public OAuthAccessToken NewOAuthAccessToken(OAuthRequestToken requestToken, out Identity identity) {
            var response = _oauth.GetAccessTokenResponse(requestToken);
            if(!response.IsSuccessful) {
                throw new DreamResponseException(response, "Failed to retrieve oauth access token");
            }
            var pairs = XUri.ParseParamsAsPairs(response.ToText())
                .ToDictionary(p => p.Key, p => p.Value);
            identity = GetIdentityByUserId(pairs["user_id"]);
            var accessToken = new OAuthAccessToken(pairs["oauth_token"], pairs["oauth_token_secret"]);
            return accessToken;
        }
        #endregion

        private IMessage NewFromSearchResult(XDoc result, TimeSpan? expiration = null) {
            XUri uri;
            XUri.TryParse(result["profile_image_url"].AsText, out uri);
            return new Message(_guidGenerator, _dateTime, expiration) {
                Source = Source.Twitter,
                SourceMessageId = result["id_str"].AsText,
                SourceInReplyToMessageId = result["in_reply_to_status_id_str"].AsText,
                SourceInReplyToIdentityId = result["to_user_id_str"].AsText,
                Body = result["text"].AsText,
                Author = new Identity {
                    Id = result["from_user_id_str"].AsText,
                    Name = result["from_user"].AsText,
                    Avatar = uri != null ? uri.ToUri() : null
                },
                SourceTimestamp = DateTime.Parse(result["created_at"].AsText)
            };
        }

        private IMessage NewFromStatusUpdate(XDoc result) {
            return new Message(_guidGenerator, _dateTime) {
                Source = Source.Twitter,
                SourceMessageId = result["id_str"].AsText,
                SourceInReplyToMessageId = result["in_reply_to_status_id_str"].AsText,
                SourceInReplyToIdentityId = result["to_user_id_str"].AsText,
                Body = result["text"].AsText,
                Author = GetIdentityByUserId(result["user/id_str"].AsText),
                SourceTimestamp = _dateTime.UtcNow
            };
        }
    }
}