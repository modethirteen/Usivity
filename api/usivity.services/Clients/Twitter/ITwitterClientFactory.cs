using Usivity.Entities.Connections;
using Usivity.Services.Clients.OAuth;

namespace Usivity.Services.Clients.Twitter {

    public interface ITwitterClientFactory {

        //--- Methods ---
        IOAuthAccessClient NewTwitterOAuthAccessClient();
        ITwitterClient NewTwitterClient(ITwitterConnection connection);
    }
}
