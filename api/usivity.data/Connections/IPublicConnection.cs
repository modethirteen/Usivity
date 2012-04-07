using System.Collections.Generic;
using Usivity.Data.Entities;

namespace Usivity.Data.Connections {

    public interface IPublicConnection {

        //--- Methods ---
        void SetNewSubscriptionUri(Subscription subscription);
        IEnumerable<Message> GetMessages(Subscription subscription);
    }
}
