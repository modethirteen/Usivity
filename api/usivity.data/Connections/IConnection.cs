using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Data.Connections {

    public interface IConnection {

        //--- Properties ---
        string Id { get; }
        Identity Identity { get; }
        SourceType Source { get; }

        //--- Methods ---
        IEnumerable<Message> GetMessages();
        Message PostReplyMessage(Message message, User user, string reply);
        XDoc ToDocument();
        void Update(XDoc config);
    }
}
