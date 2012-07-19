using Usivity.Entities.Connections;

namespace Usivity.Services.Clients.Email {

    public interface IEmailClientFactory {

        //--- Methods ---
        IEmailClient NewEmailClient(IEmailConnection connection);
    }
}
