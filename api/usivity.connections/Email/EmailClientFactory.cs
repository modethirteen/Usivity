using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Util;

namespace Usivity.Connections.Email {

    public class EmailClientFactory : IConnectionFactory {

        //--- Fields ---
        private readonly XDoc _config;
        private readonly IGuidGenerator _guidGenerator;

        //--- Constructors ---
        public EmailClientFactory(XDoc config, IGuidGenerator guidGenerator) {
            _config = config;
            _guidGenerator = guidGenerator;
        }

        //--- Methods ---
        public IConnection NewConnection(IOrganization organization) {
            return new EmailClient(_guidGenerator, organization, _config);
        }
    }
}
