using MindTouch.Dream;
using MindTouch.OAuth;

namespace Usivity.Services.Clients.Twitter {

    public class TwitterClientConfig {

        //--- Properties ---
        public string OAuthConsumerKey { get; set; }
        public string OAuthConsumerSecret { get; set; }
        public XUri OAuthCallbackBaseUri { get; set; }
        public IOAuthNonceFactory OAuthNonceFactory { get; set; }
        public IOAuthTimeStampFactory OAuthTimeStampFactory { get; set; }
    }
}
