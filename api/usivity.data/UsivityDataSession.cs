using MindTouch.Dream;
using MindTouch.Xml;
using MongoDB.Driver;

namespace Usivity.Data {

    public class UsivityDataSession {

        //--- Class Fields ---
        private static XDoc _config;
        private static MongoDatabase _instance;

        //--- Class Properties ---
        public static MongoDatabase CurrentSession {
            get {
                _instance = GetInstance();
                return _instance;
            }
        }

        //--- Class Methods ---
        public static void Initialize(XDoc config) {
            _config = config;
        }

        private static MongoDatabase GetInstance() {
            if(_instance != null) {
                return _instance;
            }
            if(_config == null) {
                throw new DreamException("Current data session has not been initialized");
            }
            var sb = new MongoConnectionStringBuilder {
                Server = new MongoServerAddress(_config["host"].AsText),
            };
            return new MongoServer(sb.ToServerSettings())
                .GetDatabase(_config["database"].AsText);        
        }        
    }
}
