using System;

namespace Usivity.Services.Clients.Email {

    public interface IEmailClient : IClient {

        //--- Methods --- 
        bool AreEmailConnectionCredentialsValid(out Exception clientErrorResponse);
    }
}
