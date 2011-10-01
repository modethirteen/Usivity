using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using MindTouch.Dream;
using MindTouch.Xml;

namespace Usivity.Core.Libraries {

	public class OAuth {

        private readonly Random Random = new Random();

        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthCallbackKey = "oauth_callback";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthSignatureKey = "oauth_signature";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";

	    public const string SignatureMethod = "HMAC-SHA1";
	    public const string Version = "1.0";
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public XUri RequestUri { get; private set; }
        public XUri AuthorizeUri { get; private set; }
        public XUri AccessTokenUri { get; private set; }
        public XUri CallbackUri { get; private set; }

        public OAuth(XDoc config) {
            ConsumerKey = config["consumer-key"].AsText;
            ConsumerSecret = config["consumer-secret"].AsText;
            RequestUri = config["request-uri"].AsUri;
            AuthorizeUri = config["authorize-uri"].AsUri;
            AccessTokenUri = config["access-token-uri"].AsUri;
            CallbackUri = config["callback-uri"].AsUri;
        }

        private string GenerateTimeStamp() {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();            
        }

        private string GenerateNonce() {
            return Random.Next(123400, 9999999).ToString();            
        }
	}
}
