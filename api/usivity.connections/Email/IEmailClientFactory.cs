using Usivity.Entities.Connections;

namespace Usivity.Connections.Email {

    public interface IEmailClientFactory {

        //--- Methods ---
        IClient NewClient(IEmailConnection connection);
    }
}
