using Usivity.Data.Entities;

namespace Usivity.Data {

    public interface IUsivityDataSession {
        
        //--- Properties ---
        ContactDataAccess Contacts { get; }
        UserDataAccess Users { get; }
        SubscriptionDataAccess Subscriptions { get; }
        OrganizationDataAccess Organizations { get; }

        //--- Methods ---
        MessageDataAccess GetMessageStream(Organization organization);
    }
}
