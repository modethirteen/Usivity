using System;
using System.Collections.Generic;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Services.Clients {

    public interface IClient {

        //--- Methods ---
        IEnumerable<IMessage> GetNewMessages(TimeSpan? expiration);
        IMessage PostNewReplyMessage(IUser user, IMessage message, string reply);
        Contact NewContact(Identity identity);
    }
}
