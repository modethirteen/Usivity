using System.Collections.Generic;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Connections {

    public interface IPublicConnection {

        //--- Methods ---
        void SetNewSubscriptionUri(Subscription subscription);
        IEnumerable<Message> GetMessages(Subscription subscription);
        Identity GetIdentity(string name);
    }
}
