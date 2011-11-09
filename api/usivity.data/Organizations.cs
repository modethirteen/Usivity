using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public IEnumerable<Organization> GetOrganizations(User user = null) {
            QueryComplete query = null;
            if(user != null) {
                var ids = BsonArray.Create(user.GetOrganizationIds());
                query = Query.In("_id", ids);
            }
            return _organizations.FindAs<Organization>(query);
        }

        public Organization GetOrganization(string id, User user = null) {
            if(user != null && user.GetOrganizationIds().ToList().Find(o => o == id) == null) {
                return null;
            }
            return _organizations.FindOneByIdAs<Organization>(id);
        }

        public void SaveOrganization(Organization organization) {
            SaveEntity(_organizations, organization);
        }
    }
}