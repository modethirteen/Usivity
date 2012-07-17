using System;
using System.Collections.Generic;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Connections {

    public interface IPublicConnection {

        //--- Methods ---
        void SetNewSubscriptionQuery(Subscription subscription);
        IEnumerable<IMessage> GetMessages(IGuidGenerator guidGenerator, IDateTime dateTime, Subscription subscription, TimeSpan expiration);
        Identity GetIdentity(string name);
    }
}
