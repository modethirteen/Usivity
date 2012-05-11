using System.Collections.Generic;
using Usivity.Entities;

namespace Usivity.Data {

    public interface ISubscriptionDataAccess {

        //--- Methods ---
        IEnumerable<Subscription> Get(IOrganization organization);
        Subscription Get(string id, IOrganization organization);
        void Save(Subscription subscription);
        void Delete(Subscription subscription);
    }
}
