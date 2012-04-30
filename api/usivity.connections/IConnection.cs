using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Connections {

    public interface IConnection {

        //--- Properties ---
        string Id { get; }
        string OrganizationId { get; }
        Identity Identity { get; }
        Source Source { get; }
        bool Active { get; }

        //--- Methods ---
        IEnumerable<Message> GetMessages();
        Message PostReplyMessage(Message message, User user, string reply);
        XDoc ToDocument();
        void Update(XDoc config);
    }
}
