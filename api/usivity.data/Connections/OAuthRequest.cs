using System;
using MindTouch.Dream;

namespace Usivity.Data.Connections {

    public class OAuthRequest : IOAuthRequest {
     
        //--- Properties ---
        public Uri AuthorizeUri { get; private set; }
        public string Token { get; private set; }
        public string TokenSecret { get; private set; }
        public string Verifier { get; set; }
        public DateTime Created { get; private set; }

        //--- Constructors ---
        public OAuthRequest(XUri authorize, string token, string secret) {
            AuthorizeUri = authorize.ToUri();
            Token = token;
            TokenSecret = secret;
            Created = DateTime.UtcNow;
        }
    }
}
