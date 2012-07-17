using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;

namespace Usivity.Data {

    public class UserDataAccess : IUserDataAccess {

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public UserDataAccess(MongoDatabase db) {
            _db = db.GetCollection<Contact>("users");
        }

        //--- Methods ---
        public IEnumerable<User> Get(IOrganization organization = null) {
            if(organization == null) {
                return _db.FindAllAs<User>();    
            }
            var query = Query.Exists("_organizations." + organization.Id, true);
            return _db.FindAs<User>(query);
        }

        public User Get(string id, IOrganization organization = null) {
            QueryComplete query;
            if(organization != null) {
                query = Query.And(
                    Query.EQ("_id", id),
                    Query.Exists("_organizations." + organization.Id, true)
                    );
            }
            else {
                query = Query.EQ("_id", id);
            }
            return _db.FindOneAs<User>(query);
        }

        public User GetAuthenticated(string name, string password) {
            var query = Query.And(
                Query.EQ("Name", name),
                Query.EQ("Password", password)
                );
            return _db.FindOneAs<User>(query);
        }

        public bool Exists(string name) {
            var query = Query.EQ("Name", name);
            return _db.FindOneAs<User>(query) != null;
        }

        public void Save(User user) {
            _db.Save(user, SafeMode.True);
        }
    }
}
