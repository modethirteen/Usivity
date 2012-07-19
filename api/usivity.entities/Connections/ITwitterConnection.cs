using MindTouch.OAuth;

namespace Usivity.Entities.Connections {

    public interface ITwitterConnection : IConnection {

        //--- Properties ---
        OAuthRequestToken OAuthRequest { get; set;}
        OAuthAccessToken OAuthAccess { get; set; }
    }
}
