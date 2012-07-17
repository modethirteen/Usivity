using System;
using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Connections {

    public interface IConnection {

        //--- Properties ---
        string Id { get; }
        string OrganizationId { get; }
        Identity Identity { get; }
        Source Source { get; }
        bool Active { get; }

        //--- Methods ---
        IEnumerable<IMessage> GetMessages(IGuidGenerator guidGenerator, IDateTime dateTime, TimeSpan? expiration);
        IMessage PostReplyMessage(IGuidGenerator guidGenerator, IDateTime dateTime, IMessage message, User user, string reply);
        XDoc ToDocument();
        void Update(XDoc config);
    }
}
