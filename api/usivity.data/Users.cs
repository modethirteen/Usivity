using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public IEnumerable<User> GetUsers(Organization organization) {
            var query = Query.In("Organization", BsonArray.Create(organization.Id));
            return _users.FindAs<User>(query);
        }

        public User GetUser(string id, Organization organization = null) {
            var query = new QueryDocument {
                {"_id", id}
            };
            if(organization != null) {
                query.Add("Organization", organization.Id);
            }
            return _users.FindOneAs<User>(query);
        }

        public User GetAnonymousUser() {
            var query = Query.EQ("Name", User.ANONYMOUS_USER);
            return _users.FindOneAs<User>(query);
        }

        public User GetAuthenticatedUser(string name, string password) {
            return null;    
        }

        public void SaveUser(User user) {
            SaveEntity(_users, user);
        }
    }
}
