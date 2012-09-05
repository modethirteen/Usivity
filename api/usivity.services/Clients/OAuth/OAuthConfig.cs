using MindTouch.OAuth;

namespace Usivity.Services.Clients.OAuth {

    public class OAuthConfig {

        //--- Properties ---
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public IOAuthNonceFactory NonceFactory { get; set; }
        public IOAuthTimeStampFactory TimeStampFactory { get; set; }
    }
}
