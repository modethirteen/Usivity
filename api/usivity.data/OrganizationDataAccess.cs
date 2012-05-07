using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;

namespace Usivity.Data {

    public class OrganizationDataAccess : IOrganizationDataAccess {

        //--- Class Methods ---
        public static void RegisterEntityClassMap() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(Organization))) {
                BsonClassMap.RegisterClassMap<Organization>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_defaultConnections");
                });
            }
        }

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public OrganizationDataAccess(MongoDatabase db) {
            _db = db.GetCollection<Contact>("organizations");
        }

        //--- Methods ---
        public IEnumerable<IOrganization> Get() {
            return _db.FindAllAs<Organization>();
        }

        public IEnumerable<IOrganization> Get(User user) {
            var ids = BsonArray.Create(user.GetOrganizationIds());
            var query = Query.In("_id", ids);
            return _db.FindAs<Organization>(query);
        }

        public IOrganization Get(string id, User user = null) {
            if(user != null && user.GetOrganizationIds().ToList().Find(o => o == id) == null) {
                return null;
            }
            return _db.FindOneByIdAs<Organization>(id);
        }

        public void Save(IOrganization organization) {
            _db.Save(organization, SafeMode.True);
        }
    }
}