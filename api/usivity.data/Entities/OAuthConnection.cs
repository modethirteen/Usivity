using MindTouch.Dream;

namespace Usivity.Data.Entities {

    public class OAuthConnection : IConnection {

        //--- Properties ---
        public string Token { get; private set; }
        public string Secret { get; private set; }

        //--- Constructors ---
        public OAuthConnection(string token, string secret) {
            Token = token;
            Secret = secret;
        }

        //--- Methods ---
        public DreamHeaders GetHeaders() {
            var headers = new DreamHeaders();
            headers["oauth_token"] = Token;
            headers["oauth_token_secret"] = Secret;
            return headers;
        }
    }
}
