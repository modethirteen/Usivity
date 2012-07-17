using MindTouch.OAuth;
using Usivity.Entities.Types;

namespace Usivity.Services.Clients.Twitter {
    
    public interface ITwitterClient : IClient, IPublicClient {

        //--- Methods ---
        OAuthRequestToken NewOAuthRequestToken();
        OAuthAccessToken NewOAuthAccessToken(OAuthRequestToken requestToken, out Identity identity);
    }
}
