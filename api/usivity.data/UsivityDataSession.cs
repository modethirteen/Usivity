using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Usivity.Data.Connections;
using Usivity.Data.Entities;

namespace Usivity.Data {

    public class UsivityDataSession : IUsivityDataSession, IDisposable {
        
        //--- Class Fields ---
        private static bool _registeredClassMaps;

        //--- Class Constructors ---
        public static IUsivityDataSession NewUsivityDataSession(string connection) {
            if(!_registeredClassMaps) {
                RegisterClassMaps();
                _registeredClassMaps = true;
            }
            return new UsivityDataSession(connection);
        }

        //--- Class Methods ---
        public static string GenerateEntityId(IEntity entity) {
            return ObjectId.GenerateNewId().ToString();
        }

        public static string GenerateConnectionId(IConnection connection) {
            return ObjectId.GenerateNewId().ToString();
        }

        protected static void RegisterClassMaps() {

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
        }

        //--- Properties ---
        public ContactDataAccess Contacts { get; private set; }
        public UserDataAccess Users { get; private set; }
        public SubscriptionDataAccess Subscriptions { get; private set; }
        public OrganizationDataAccess Organizations { get; private set; }

        //--- Fields ---
        private MongoDatabase _db;

        //--- Constructors ---
        protected UsivityDataSession(string connection) {
            _db = MongoDatabase.Create(connection);
            Contacts = new ContactDataAccess(_db);
            Users = new UserDataAccess(_db);
            Subscriptions = new SubscriptionDataAccess(_db);
            Organizations = new OrganizationDataAccess(_db);
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
