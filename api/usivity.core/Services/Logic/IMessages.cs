using System;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

    public interface IMessages {

        //--- Methods ---
        Message GetMessage(string id);
        XDoc GetConversationsXml(Contact contact);
        XDoc GetMessageXml(Message message, string relation = null);
        XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset);
        XDoc GetMessageVerboseXml(Message message);
        XDoc GetMessageParentsXml(Message message);
        XDoc GetMessageChildrenXml(Message message);
        XDoc PostReply(Message message, string reply);
        void SaveMessage(Message message);
        void DeleteMessage(Message message);
    }
}
