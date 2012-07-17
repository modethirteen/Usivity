using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Util;

namespace Usivity.Connections.Twitter {

    public class TwitterClientFactory : IConnectionFactory {

        //--- Fields ---
        private readonly XDoc _config;
        private readonly IGuidGenerator _guidGenerator;

        //--- Constructors ---
        public TwitterClientFactory(XDoc config, IGuidGenerator guidGenerator) {
            _config = config;
            _guidGenerator = guidGenerator;
        }

        //--- Methods ---
        public IConnection NewConnection(IOrganization organization) {
            return new TwitterClient(_guidGenerator, organization, _config);
        }
    }
}
