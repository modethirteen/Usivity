using System;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services.Logic {

    public interface IMessages {

        //--- Methods ---
        Message GetMessage(string id);
        XDoc GetConversationsXml(Contact contact);
        XDoc GetMessageXml(Message message, string relation = null);
        XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset);
        XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset, Source source);
        XDoc GetMessageVerboseXml(Message message);
        XDoc GetMessageParentsXml(Message message);
        XDoc GetMessageChildrenXml(Message message);
        XDoc PostReply(Message message, string reply);
        void SaveMessage(Message message);
        void DeleteMessage(Message message);
    }
}
