using Usivity.Entities.Connections;

namespace Usivity.Services.Clients.Twitter {

    public interface ITwitterClientFactory {

        //--- Methods ---
        ITwitterClient NewTwitterClient(ITwitterConnection connection);
    }
}
