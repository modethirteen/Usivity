using MindTouch.Xml;
using Usivity.Entities;

namespace Usivity.Connections.Email {

    public class EmailConnectionFactory : IConnectionFactory {

        //--- Fields ---
        private readonly XDoc _config;

        //--- Constructors ---
        public EmailConnectionFactory(XDoc config) {
            _config = config;
        }

        //--- Methods ---
        public IConnection NewConnection(IOrganization organization) {
            return new EmailConnection(_config, organization);
        }
    }
}
