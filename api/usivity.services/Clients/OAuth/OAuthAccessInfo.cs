using MindTouch.OAuth;
using Usivity.Entities.Types;

namespace Usivity.Services.Clients.OAuth {

    public class OAuthAccessInfo {

        //--- Properties ---
        public OAuthAccessToken Token { get; private set; }
        public Identity Identity { get; private set; }

        //--- Constructors ---
        public OAuthAccessInfo(OAuthAccessToken token, Identity identity) {
            Token = token;
            Identity = identity;
        }
    }
}
