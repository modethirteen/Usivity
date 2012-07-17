using System.Collections.Generic;

namespace Usivity.Entities {

    public interface IUser : IEntity {

        //--- Properties ---
        string Name { get; }
        string Password { get; set; }
        string CurrentOrganization { get; set; }
        bool IsAnonymous { get; }

        //--- Methods ---
        User.UserRole GetOrganizationRole(IOrganization organization);
        void SetOrganizationRole(IOrganization organization, User.UserRole role);
        IEnumerable<string> GetOrganizationIds();
    }
}
