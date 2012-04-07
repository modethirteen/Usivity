using System;
using MindTouch.Dream;
using MindTouch.Xml;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Usivity.Data.Connections;
using Usivity.Data.Entities;

namespace Usivity.Data {

    public class UsivityDataSession : IUsivityDataSession, IDisposable {

        //--- Class Properties ---
        public static UsivityDataSession CurrentSession {
            get {
                _instance = GetInstance();
                return _instance;
            }
        }

        //--- Class Fields ---
        private static XDoc _config;
        private static UsivityDataSession _instance;

        //--- Class Methods ---
        public static void Initialize(XDoc config) {
            _config = config;
        }

        public static string GenerateEntityId(IEntity entity) {
            return ObjectId.GenerateNewId().ToString();
        }

        public static string GenerateConnectionId(IConnection connection) {
            return ObjectId.GenerateNewId().ToString();
        }

        private static UsivityDataSession GetInstance() {
            if(_instance != null) {
                return _instance;
            }
            if(_config == null) {
                throw new DreamException("Current data session has not been initialized");
            }
            var db = MongoDatabase.Create(_config["connection"].AsText);

            // register entity maps
            ContactDataAccess.RegisterClassMap();
            UserDataAccess.RegisterClassMap();
            SubscriptionDataAccess.RegisterClassMap();
            OrganizationDataAccess.RegisterClassMap();
            
            // register connection maps
            BsonClassMap.RegisterClassMap<TwitterConnection>(cm => {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.MapField("_oauth");
                cm.MapField("_oauthRequest");
            });
            return new UsivityDataSession(db);
        }

        //--- Properties ---
        public ContactDataAccess Contacts { get; private set; }
        public UserDataAccess Users { get; private set; }
        public SubscriptionDataAccess Subscriptions { get; private set; }
        public OrganizationDataAccess Organizations { get; private set; }

        //--- Fields ---
        private MongoDatabase _db;

        //--- Constructors ---
        private UsivityDataSession(MongoDatabase db) {
            _db = db; 
            Contacts = new ContactDataAccess(db);
            Users = new UserDataAccess(db);
            Subscriptions = new SubscriptionDataAccess(db);
            Organizations = new OrganizationDataAccess(db);
        }

        //--- Methods ---
        public MessageDataAccess GetMessageStream(Organization organization) {
            return new MessageDataAccess(_db, organization);
        }

        public void Dispose() {
            _db = null;
        }
    }
}
