using MindTouch.Xml;

namespace Usivity.Services.Clients.Twitter {

    public class TwitterAuthorization {

        //--- Properties ---
        public string OAuthRequestToken { get; set; }
        public string OAuthVerifier { get; set; }

        //--- Constructors ---
        public TwitterAuthorization(XDoc config) {
            OAuthRequestToken = config["token"].AsText;
            OAuthVerifier = config["verifier"].AsText;
        }
    }
}
