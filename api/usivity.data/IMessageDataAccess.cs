using System;
using System.Collections.Generic;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Data {

    public interface IMessageDataAccess {

        //--- Methods ---
        IEnumerable<IMessage> GetStream(DateTime start, DateTime end, int count, int offset, Source? source);
        IEnumerable<IMessage> GetConversations(Contact contact);
        IEnumerable<IMessage> GetChildren(IMessage message);
        IMessage Get(string id);
        IMessage Get(Source source, string sourceId);
        void Save(IMessage message);
        void Queue(IMessage message);
        void Delete(IMessage message);
        void RemoveExpired(IDateTime dateTime);
        long GetCount();
    }
}
