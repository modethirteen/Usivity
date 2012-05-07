using System;
using MongoDB.Driver;
using Usivity.Entities;

namespace Usivity.Data {

    public class UsivityDataCatalog : IUsivityDataCatalog, IDisposable {
        
        //--- Class Constructors ---
        public static IUsivityDataCatalog NewUsivityDataCatalog(string connection) {
            ContactDataAccess.RegisterEntityClassMap();
            UserDataAccess.RegisterEntityClassMap();
            SubscriptionDataAccess.RegisterEntityClassMap();
            OrganizationDataAccess.RegisterEntityClassMap();
            ConnectionDataAccess.RegisterConnectionClassMaps();
            return new UsivityDataCatalog(connection);
        }

        //--- Properties ---
        public IContactDataAccess Contacts { get; private set; }
        public IUserDataAccess Users { get; private set; }
        public ISubscriptionDataAccess Subscriptions { get; private set; }
        public IOrganizationDataAccess Organizations { get; private set; }
        public IConnectionDataAccess Connections { get; private set; }

        //--- Fields ---
        private MongoDatabase _db;

        //--- Constructors ---
        protected UsivityDataCatalog(string connection) {
            _db = MongoDatabase.Create(connection);
            Contacts = new ContactDataAccess(_db);
            Users = new UserDataAccess(_db);
            Subscriptions = new SubscriptionDataAccess(_db);
            Organizations = new OrganizationDataAccess(_db);
            Connections = new ConnectionDataAccess(_db);
        }

        //--- Methods ---
        public IMessageDataAccess GetMessageStream(IOrganization organization) {
            return new MessageDataAccess(_db, organization);
        }

        public void Dispose() {
            _db = null;
        }
    }
}
