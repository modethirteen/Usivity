using System;
using MindTouch.Dream;
using MindTouch.OAuth;
using Usivity.Entities.Connections;
using Usivity.Services.Clients.OAuth;
using Usivity.Util;

namespace Usivity.Services.Clients.Twitter {

    public class TwitterClientFactory : ITwitterClientFactory {

#if DEBUG
        private const string OAUTH_REQUEST_TOKEN_URI = "http://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "http://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "http://api.twitter.com/oauth/authorize";
#else
        private const string OAUTH_REQUEST_TOKEN_URI = "https://api.twitter.com/oauth/request_token";
        private const string OAUTH_ACCESS_TOKEN_URI = "https://api.twitter.com/oauth/access_token";
        private const string OAUTH_AUTHORIZE_URI = "https://api.twitter.com/oauth/authorize";
#endif

        //--- Fields ---
        private readonly OAuthConfig _config;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTime _dateTime;

        //--- Constructors ---
        public TwitterClientFactory(OAuthConfig config, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _config = config;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
        }

        //--- Methods ---
        public IOAuthAccessClient NewTwitterOAuthAccessClient() {
            return new TwitterOAuthClient(NewOAuthHandler());
        }

        public ITwitterClient NewTwitterClient(ITwitterConnection connection) {
            return new TwitterClient(NewOAuthHandler(), connection, _guidGenerator, _dateTime);
        }

        private IOAuthHandler NewOAuthHandler() {
             var oauthConfig = new OAuthHandlerConfig {
                ConsumerKey = _config.ConsumerKey,
                ConsumerSecret = _config.ConsumerSecret,
                AuthorizeUri = new XUri(OAUTH_AUTHORIZE_URI),
                AccessTokenUri = new XUri(OAUTH_ACCESS_TOKEN_URI),
                RequestTokenUri = new XUri(OAUTH_REQUEST_TOKEN_URI),
                NonceFactory = _config.NonceFactory,
                TimeStampFactory = _config.TimeStampFactory
            };
            return new OAuthHandler(oauthConfig); 
        }
    }
}
