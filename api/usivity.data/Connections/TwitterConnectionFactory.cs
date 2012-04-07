using MindTouch.Xml;

namespace Usivity.Data.Connections {

    public class TwitterConnectionFactory : ITwitterConnectionFactory {

        //--- Fields ---
        private readonly XDoc _config;

        //--- Constructors ---
        public TwitterConnectionFactory(XDoc config) {
            _config = config;
        }

        //--- Methods ---
        public TwitterConnection NewTwitterConnection() {
            return new TwitterConnection(_config);
        }
    }
}
