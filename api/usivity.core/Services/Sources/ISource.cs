using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Sources {

    interface ISource {

        //--- Properties ---
        string Id { get; }
        IList<Subscription> Subscriptions { get; set; }

        //--- Methods ---
        IList<Message> GetMessages(Message.MessageStreams stream);
        XDoc GetConnectionXml(User user);
        IConnection GetNewConnection(XDoc connectionRequest);
        Subscription GetNewSubscription(IEnumerable<string> constraints);
        Message PostMessageReply(Message message, string reply, IConnection connection);
    }
}
