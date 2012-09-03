using System.Linq;
using MindTouch.Dream;
using MindTouch.OAuth;
using Usivity.Entities.Types;
using Usivity.Services.Clients.OAuth;

namespace Usivity.Services.Clients.Twitter {

    class TwitterOAuthClient : IOAuthAccessClient {

        //--- Fields ---
        private readonly IOAuthHandler _oauth;

        //--- Constructors ---
        public TwitterOAuthClient(IOAuthHandler oauth) {
            _oauth = oauth; 
        }

        //--- Methods ---
        public OAuthRequestToken NewOAuthRequestToken(XUri callback) {
            var response = _oauth.GetRequestTokenResponse(callback);
            if(!response.IsSuccessful) {
                throw new DreamResponseException(response, "Failed to retrieve oauth request token");
            }
            return _oauth.GetRequestToken(response);
        }

        public OAuthAccessInfo NewOAuthAccessToken(OAuthRequestToken requestToken) {
            var response = _oauth.GetAccessTokenResponse(requestToken);
            if(!response.IsSuccessful) {
                throw new DreamResponseException(response, "Failed to retrieve oauth access token");
            }
            var pairs = XUri.ParseParamsAsPairs(response.ToText())
                .ToDictionary(p => p.Key, p => p.Value);
            var identity = new Identity { Id = pairs["user_id"] };
            var accessToken = new OAuthAccessToken(pairs["oauth_token"], pairs["oauth_token_secret"]);
            return new OAuthAccessInfo(accessToken, identity);
        }
    }
}
