using System;
using System.Collections.Generic;
using Usivity.Entities;

namespace Usivity.Services.Clients {

    public interface IPublicClient {

        //--- Methods ---
        IEnumerable<IMessage> GetNewPublicMessages(Subscription subscription, TimeSpan? expiration);
    }
}
