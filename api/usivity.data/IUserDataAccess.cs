using System.Collections.Generic;
using Usivity.Data.Entities;

namespace Usivity.Data {

    public interface IUserDataAccess {
        
        //--- Methods ---
        IEnumerable<User> Get(Organization organization = null);
        User Get(string id, Organization organization = null);
        User GetAnonymous();
        User GetAuthenticated(string name, string password);
        bool Exists(string name);
        void Save(User user);
    }
}
