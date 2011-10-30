using MindTouch.Dream;
using MindTouch.Xml;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Usivity.Data.Entities;

namespace Usivity.Data {

    public partial class UsivityDataSession {

        //--- Class Fields ---
        private static XDoc _config;
        private static UsivityDataSession _instance;

        //--- Fields ---
        private MongoDatabase _db;
        private MongoCollection _organizations;
        private MongoCollection _subscriptions;
        private MongoCollection _contacts;
        private MongoCollection _users;

        //--- Class Properties ---
        public static UsivityDataSession CurrentSession {
            get {
                _instance = GetInstance();
                return _instance;
            }
        }

        //--- Class Methods ---
        public static void Initialize(XDoc config) {
            _config = config;
        }

        private static UsivityDataSession GetInstance() {
            if(_instance != null) {
                return _instance;
            }
            if(_config == null) {
                throw new DreamException("Current data session has not been initialized");
            }
            var sb = new MongoConnectionStringBuilder {
                Server = new MongoServerAddress(_config["host"].AsText),
            };
            var db = new MongoServer(sb.ToServerSettings())
                .GetDatabase(_config["database"].AsText);
            var session = new UsivityDataSession {
                _db = db,
                _organizations = db.GetCollection<Organization>("organizations"),
                _subscriptions = db.GetCollection<Subscription>("subscriptions"),
                _contacts = db.GetCollection<Contact>("contacts"),
                _users = db.GetCollection<User>("users")
            };

            // Entity serialization maps
            BsonClassMap.RegisterClassMap<User>(cm => {
                cm.MapIdProperty("Id");
                cm.MapField("_connections");
            });
            BsonClassMap.RegisterClassMap<Contact>(cm => {
                cm.MapIdProperty("Id");
                cm.MapProperty("ClaimedByUserId");
                cm.MapProperty("FirstName");
                cm.MapProperty("LastName");
                cm.MapField("_identities");
            });
            BsonClassMap.RegisterClassMap<OAuthConnection>();

            return session;
        }

        public static string GenerateEntityId(IEntity entity) {
            return ObjectId.GenerateNewId().ToString();
        }

        private static void SaveEntity(MongoCollection collection, IEntity entity) {
            try {
                collection.Save(entity, SafeMode.True);
            }
            catch(MongoException e) {
                
                //TODO: handle errors from writing in safe mode
            }
        }
    }
}
