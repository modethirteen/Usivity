using MindTouch.Dream;
using MindTouch.OAuth;

namespace Usivity.Services.Clients.OAuth {

    public interface IOAuthAccessClient {

        //--- Methods ---
        OAuthRequestToken NewOAuthRequestToken(XUri callback = null);
        OAuthAccessInfo NewOAuthAccessToken(OAuthRequestToken requestToken);
    }
}
