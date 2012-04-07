using System.Collections.Generic;
using Usivity.Data.Entities;

namespace Usivity.Data {

    public interface IOrganizationDataAccess {

        //--- Methods ---
        IEnumerable<Organization> Get();
        IEnumerable<Organization> Get(User user);
        Organization Get(string id, User user = null);
        void Save(Organization organization);
    }
}
