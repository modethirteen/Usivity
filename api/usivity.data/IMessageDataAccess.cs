using System;
using System.Collections.Generic;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Data {

    public interface IMessageDataAccess {

        //--- Methods ---
        IEnumerable<Message> GetStream(DateTime start, DateTime end, int count, int offset, Source source);
        IEnumerable<Message> GetStream(DateTime start, DateTime end, int count, int offset);
        IEnumerable<Message> GetConversations(Contact contact);
        IEnumerable<Message> GetChildren(Message message);
        Message Get(string id);
        Message Get(Source source, string sourceId);
        void Save(Message message);
        void Queue(Message message);
        void Delete(Message message);
        void RemoveExpired();
        long GetCount();
    }
}
