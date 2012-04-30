using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Data {

    public class MessageDataAccess : IMessageDataAccess {

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public MessageDataAccess(MongoDatabase db, Organization organization) {
            _db = db.GetCollection<Message>(organization.Id + "_messages");
            var indexes = new IndexKeysBuilder().Ascending("Source", "SourceMessageId");
            _db.EnsureIndex(indexes, IndexOptions.SetUnique(true));
            _db.EnsureIndex("MessageThreadIds");
        }

        //--- Methods ---
        public IEnumerable<Message> GetStream(DateTime start, DateTime end, int count, int offset, Source source) {
            var query = Query.And(
                Query.GTE("Timestamp", start),
                Query.LTE("Timestamp", end),
                Query.EQ("OpenStream", true),
                Query.EQ("Source", source)
                );
            return GetMessages(query, count, offset);
        }

        public IEnumerable<Message> GetStream(DateTime start, DateTime end, int count, int offset) {
            var query = Query.And(
                Query.GTE("Timestamp", start),
                Query.LTE("Timestamp", end),
                Query.EQ("OpenStream", true)
                );
            return GetMessages(query, count, offset);
        }

        public IEnumerable<Message> GetConversations(Contact contact) {
            var queries = new List<IMongoQuery>();
            var identities = contact.GetSourceIdentities();
            foreach(var identity in identities.Where(i => !string.IsNullOrEmpty(i.Value.Id))) {
                var subQuery = Query.And(
                    Query.EQ("Source", identity.Key),
                    Query.EQ("Author._id", identity.Value.Id),
                    Query.EQ("ParentMessageId", "")
                    );
                queries.Add(subQuery);
            }
            var query = Query.Or(queries.ToArray());
            return _db.FindAs<Message>(query);
        }

        public IEnumerable<Message> GetChildren(Message message) {
            var query = Query.EQ("ParentMessageId", message.Id);
            return _db.FindAs<Message>(query);
        }

        public Message Get(string id) {
            return _db.FindOneByIdAs<Message>(id);
        }

        public Message Get(Source source, string sourceId) {
            var query = Query.And(
                Query.EQ("Source", source),
                Query.EQ("SourceMessageId", sourceId)
            );
            return _db.FindOneAs<Message>(query);
        }

        public void Save(Message message) {
            _db.Save(message, SafeMode.True);
        }
        
        public void Queue(Message message) {
            if(message.SourceInReplyToMessageId != null) {
                var query = Query.And(
                    Query.EQ("Source", message.Source),
                    Query.EQ("SourceMessageId", message.SourceInReplyToMessageId)
                );
                var parent = _db.FindOneAs<Message>(query);
                if(parent != null) {
                    message.ParentMessageId = parent.Id;
                }    
            }
            _db.Insert(message);
        }

        public void Delete(Message message) {
            _db.Remove(Query.EQ("_id", message.Id));
        }

        public void RemoveExpired() {
            _db.FindAndRemove(Query.LTE("Expires", DateTime.UtcNow), SortBy.Null);
        }

        public long GetCount() {
            return _db.Count();
        }

        private IEnumerable<Message> GetMessages(IMongoQuery query, int count, int offset) {
            return _db
                .FindAs<Message>(query)
                .SetLimit(count)
                .SetSkip(offset)
                .SetSortOrder(SortBy.Descending("Timestamp"));
        }
    }
}
