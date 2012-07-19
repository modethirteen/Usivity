using System.Collections.Generic;
using Usivity.Entities;

namespace Usivity.Data {

    public interface IUserDataAccess {
        
        //--- Methods ---
        IEnumerable<User> Get(IOrganization organization = null);
        User Get(string id, IOrganization organization = null);
        User GetAuthenticated(string name, string password);
        bool Exists(string name);
        void Save(User user);
    }
}
