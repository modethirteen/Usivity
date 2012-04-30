using System.Collections.Generic;
using Usivity.Entities;

namespace Usivity.Data {

    public interface IContactDataAccess {

        //--- Methods ---
        IEnumerable<Contact> Get(Organization organization);
        Contact Get(Message message);
        Contact Get(string id, Organization organization);
        void Save(Contact contact);
    }
}
