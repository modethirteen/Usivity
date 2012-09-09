using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Data {

    public class MessageDataAccess : IMessageDataAccess {

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public MessageDataAccess(MongoDatabase db, IOrganization organization) {
            _db = db.GetCollection<Message>(organization.Id + "_messages");
            var indexes = new IndexKeysBuilder().Ascending("Source", "SourceMessageId");
            _db.EnsureIndex(indexes, IndexOptions.SetUnique(true));
            _db.EnsureIndex("MessageThreadIds");
        }

        //--- Methods ---
        public IEnumerable<IMessage> GetStream(DateTime start, DateTime end, int count, int offset, Source? source) {
            var query = Query.And(
                Query.GTE("Created", start),
                Query.LTE("Created", end),
                Query.EQ("OpenStream", true)
            );
            if(source != null) {
                query = Query.And(query, Query.EQ("Source", source));
            }
            return _db
                .FindAs<Message>(query)
                .SetLimit(count)
                .SetSkip(offset)
                .SetSortOrder(SortBy.Descending("_id"));
        }

        public IEnumerable<IMessage> GetConversations(Contact contact) {
            var queries = new List<IMongoQuery>();
            var identities = contact.Identities;
            if(!identities.Any()) {
                return new Message[0];
            }
            foreach(var identity in identities.Where(i => !string.IsNullOrEmpty(i.Value.Id))) {
                var subQuery = Query.And(
                    Query.EQ("Source", identity.Key),
                    Query.EQ("Author._id", identity.Value.Id),
                    Query.EQ("ParentMessageId", BsonNull.Value)
                    );
                queries.Add(subQuery);
            }
            var query = Query.Or(queries.ToArray());
            return _db.FindAs<Message>(query).SetSortOrder(SortBy.Descending("SourceCreated"));
        }

        public IEnumerable<IMessage> GetChildren(IMessage message) {
            var query = Query.EQ("ParentMessageId", message.Id);
            return _db.FindAs<Message>(query).SetSortOrder(SortBy.Descending("SourceCreated"));
        }

        public IMessage Get(string id) {
            return _db.FindOneByIdAs<Message>(id);
        }

        public IMessage Get(Source source, string sourceId) {
            var query = Query.And(
                Query.EQ("Source", source),
                Query.EQ("SourceMessageId", sourceId)
            );
            return _db.FindOneAs<Message>(query);
        }

        public void Save(IMessage message) {
            _db.Save(message, SafeMode.True);
        }
        
        public void Queue(IMessage message) {
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

        public void Delete(IMessage message) {
            _db.Remove(Query.EQ("_id", message.Id));
        }

        public void RemoveExpired(IDateTime dateTime) {
            _db.FindAndRemove(Query.LTE("Expires", dateTime.UtcNow), SortBy.Null);
        }

        public long GetCount() {
            return _db.Count();
        }
    }
}
