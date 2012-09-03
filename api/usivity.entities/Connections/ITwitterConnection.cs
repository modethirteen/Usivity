using MindTouch.OAuth;

namespace Usivity.Entities.Connections {

    public interface ITwitterConnection : IConnection {

        //--- Properties ---
        OAuthAccessToken OAuthAccess { get; set; }
    }
}
