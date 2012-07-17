using System;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Services.Core.Logic {

    public interface IMessages {

        //--- Methods ---
        IMessage GetMessage(string id);
        XDoc GetConversationsXml(Contact contact, bool renderChildrenAsTree);
        XDoc GetMessageXml(IMessage message, string relation = null);
        XDoc GetMessageStreamXml(DateTime startTime, DateTime endTime, int count, int offset, Source? source);
        XDoc GetMessageVerboseXml(IMessage message, bool renderChildrenAsTree);
        XDoc GetMessageParentsXml(IMessage message);
        XDoc PostReply(IMessage message, string reply);
        void SaveMessage(IMessage message);
        void DeleteMessage(IMessage message);
    }
}
