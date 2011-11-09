using System.Collections.Generic;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public IEnumerable<User> GetUsers(Organization organization) {
            var query = Query.Exists("_organizations." + organization.Id, true);
            return _users.FindAs<User>(query);
        }

        public IEnumerable<User> GetUsers() {
            return _users.FindAllAs<User>();
        }

        public User GetUser(string id, Organization organization = null) {
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
            return _users.FindOneAs<User>(query);
        }

        public bool UserExists(string name) {
            var query = Query.EQ("Name", name);
            return _users.FindOneAs<User>(query) != null;
        }

        public User GetAnonymousUser() {
            var query = Query.EQ("Name", User.ANONYMOUS_USER);
            return _users.FindOneAs<User>(query);
        }

        public User GetAuthenticatedUser(string name, string password) {
            var query = Query.And(
                Query.EQ("Name", name),
                Query.EQ("Password", password)
                );
            return _users.FindOneAs<User>(query);
        }

        public void SaveUser(User user) {
            SaveEntity(_users, user);
        }
    }
}
