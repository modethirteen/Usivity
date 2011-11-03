using System.Collections.Generic;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public IEnumerable<Organization> GetOrganizations() {
            return _organizations.FindAllAs<Organization>();
        }

        public Organization GetOrganization(string id) {
            return _organizations.FindOneByIdAs<Organization>(id);
        }

        public void SaveOrganization(Organization organization) {
            SaveEntity(_organizations, organization);
        }
    }
}