using MindTouch.Xml;
using Usivity.Entities;

namespace Usivity.Connections.Twitter {

    public class TwitterConnectionFactory : IConnectionFactory {

        //--- Fields ---
        private readonly XDoc _config;

        //--- Constructors ---
        public TwitterConnectionFactory(XDoc config) {
            _config = config;
        }

        //--- Methods ---
        public IConnection NewConnection(Organization organization) {
            return new TwitterConnection(_config, organization);
        }
    }
}
