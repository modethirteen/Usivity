using Usivity.Entities;

namespace Usivity.Data {

    public interface IUsivityDataCatalog {
        
        //--- Properties ---
        IContactDataAccess Contacts { get; }
        IUserDataAccess Users { get; }
        ISubscriptionDataAccess Subscriptions { get; }
        IOrganizationDataAccess Organizations { get; }
        IConnectionDataAccess Connections { get; }

        //--- Methods ---
        IMessageDataAccess GetMessageStream(Organization organization);
    }
}
