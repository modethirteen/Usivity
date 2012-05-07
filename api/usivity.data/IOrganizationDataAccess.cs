using System.Collections.Generic;
using Usivity.Entities;

namespace Usivity.Data {

    public interface IOrganizationDataAccess {

        //--- Methods ---
        IEnumerable<IOrganization> Get();
        IEnumerable<IOrganization> Get(User user);
        IOrganization Get(string id, User user = null);
        void Save(IOrganization organization);
    }
}
