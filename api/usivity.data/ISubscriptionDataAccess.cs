using System.Collections.Generic;
using Usivity.Data.Entities;

namespace Usivity.Data {
    
    public interface ISubscriptionDataAccess {

        //--- Methods ---
        IEnumerable<Subscription> Get(Organization organization);
        Subscription Get(string id, Organization organization);
        void Save(Subscription subscription);
        void Delete(Subscription subscription);
    }
}
