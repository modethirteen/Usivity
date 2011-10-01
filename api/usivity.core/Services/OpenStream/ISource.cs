using System;
using System.Collections.Generic;

namespace Usivity.Core.Services.OpenStream {

    interface ISource {

        //--- Properties ---
        string Name { get; }
        IList<Subscription> Subscriptions { get; set; }

        //--- Methods ---
        IList<Message> GetMessages();
        Uri GenerateUriWithConstraints(IList<string> constraints);
    }
}
