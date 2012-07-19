using Usivity.Entities.Connections;
using Usivity.Util;

namespace Usivity.Services.Clients.Email {

    public class EmailClientFactory : IEmailClientFactory {

        //--- Fields ---
        private readonly EmailClientConfig _config;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTime _dateTime;

        //--- Constructors ---
        public EmailClientFactory(EmailClientConfig config, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _config = config;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
        }

        //--- Methods ---
        public IEmailClient NewEmailClient(IEmailConnection connection) {
            return new EmailClient(_config, connection, _guidGenerator, _dateTime);
        }
    }
}
