using System;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using MindTouch.Dream;
using MindTouch.Xml;

namespace Usivity.Data.Connections {

	public class OAuth {

        //--- Constants ---
	    public const string SignatureMethod = "HMAC-SHA1";
	    public const string Version = "1.0";

        //--- Properties ---
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public Uri RequestUri { get; private set; }
        public Uri AuthorizeUri { get; private set; }
        public Uri AccessTokenUri { get; private set; }
        public Uri CallbackUri { get; private set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        //--- Constructors ---
        public OAuth(XDoc config) {
            ConsumerKey = config["consumer.key"].AsText;
            ConsumerSecret = config["consumer.secret"].AsText;
            RequestUri = config["uri.token.request"].AsUri;
            AuthorizeUri = config["uri.authorize"].AsUri;
            AccessTokenUri = config["uri.token.access"].AsUri;
        }

        //--- Methods ---
        public DreamMessage GetRequestToken() {
            return Plug.New(RequestUri)
                .WithPreHandler(AuthorizationHeader)
                .Post();
        }

        public DreamMessage GetAccessToken(IOAuthRequest request) {
            return Plug.New(AccessTokenUri)
                .WithHeader("oauth_token", request.Token)
                .WithHeader("oauth_token_secret", request.TokenSecret)
                .WithHeader("oauth_verifier", request.Verifier)
                .WithPreHandler(AuthorizationHeader)
                .Post();
        }

        public DreamMessage AuthorizationHeader(string verb, XUri uri, XUri normalizedUri, DreamMessage message) {
            var headers = new SortedDictionary<string, string> {
                {"oauth_version", Version},
                {"oauth_consumer_key", ConsumerKey},
                {"oauth_nonce", GenerateNonce()},
                {"oauth_signature_method", SignatureMethod},
                {"oauth_timestamp", GenerateTimeStamp()}
            };
            if(!string.IsNullOrEmpty(AccessToken)) {
                headers["oauth_token"] = AccessToken;
            }
            if(!string.IsNullOrEmpty(AccessTokenSecret)) {
                headers["oauth_token_secret"] = AccessTokenSecret;
            }

            // append oauth headers that may have come from stored connection data
            foreach (var header in message.Headers.Where(header => header.Key.StartsWithInvariant("oauth_"))) {
                headers.Add(header.Key, header.Value);
            }
            headers.Add("oauth_signature", GenerateSignature(verb, uri, headers));
            message.Headers.Authorization = "OAuth " +
                string.Join(",", headers.Select(header => XUri.Encode(header.Key) + "=" + XUri.Encode(header.Value)).ToArray());
            return message;
        }

        private string GenerateSignature(string verb, XUri uri, SortedDictionary<string, string> headers) {
            var headerString = string.Join("&", headers.Select(header => header.Key + "=" + header.Value).ToArray());
            var baseString = string.Format(
                "{0}&{1}&{2}",
                XUri.Encode(verb.ToUpperInvariant()),
                XUri.Encode(uri.ToString()),
                XUri.Encode(headerString)
                );

            var signingKey = ConsumerSecret + "&" + headers.TryGetValue("oauth_token_secret", string.Empty);
            var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));

            return Convert.ToBase64String(
                hmac.ComputeHash(
                Encoding.ASCII.GetBytes(baseString)));
        }

	    private static string GenerateTimeStamp() {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();            
        }

        private static string GenerateNonce() {
            return new Random().Next(123400, 9999999).ToString();            
        }
	}
}
