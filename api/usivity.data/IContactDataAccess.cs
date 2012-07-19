using System.Collections.Generic;
using Usivity.Entities;

namespace Usivity.Data {

    public interface IContactDataAccess {

        //--- Methods ---
        IEnumerable<Contact> Get(IOrganization organization);
        Contact Get(IMessage message);
        Contact Get(string id, IOrganization organization);
        void Save(Contact contact);
    }
}
