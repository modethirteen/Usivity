using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public IEnumerable<Message> GetOpenStreamMessages(Organization organization, User user, int count, double minutesUntilNextAccess) {
            var openstream = GetMessagesCollection(organization);
            var query = Query.And(
                Query.LTE("NextAccess", DateTime.UtcNow),
                Query.EQ("Stream", Message.MessageStreams.Open)
                );
            var messages = openstream
                .FindAs<Message>(query)
                .SetLimit(count)
                .SetSortOrder("NextAccess");
            foreach(var message in messages) {
                message.NextAccess = DateTime.UtcNow.AddMinutes(minutesUntilNextAccess);
                openstream.Save(message);
            }
            return messages;
        }

        public IEnumerable<Message> GetUserStreamMessages(Organization organization, User user, int count) {
            var openstream = GetMessagesCollection(organization);
            var contacts = GetContacts(user);
            var conversations = new List<string>();
            foreach(var contact in contacts) {
                var contactConverstations = contact.GetConversations();
                if(contactConverstations != null && contactConverstations.Count() > 0) {
                    conversations.AddRange(contact.GetConversations());   
                }
            }
            var ids = BsonArray.Create(conversations);
            var query = Query.In("_id", ids);
            var messages = openstream.FindAs<Message>(query);
            return messages;
        }

        public IEnumerable<Message> GetMessageChildren(Organization organization, Message message) {
            var openstream = GetMessagesCollection(organization);
            var query = Query.EQ("ParentMessageId", message.Id);
            return openstream.FindAs<Message>(query);
        }

        public void SaveMessage(Message message, Organization organization) {
            var openstream = GetMessagesCollection(organization);
            SaveEntity(openstream, message);
        }
        
        public void RemoveExpiredOpenStreamMessages() {
            var organizations = GetOrganizations();
            var query = Query.And(
                Query.LTE("Expires", DateTime.UtcNow),
                Query.EQ("Stream", Message.MessageStreams.Open)
                );
            foreach(var messages in organizations.Select(GetMessagesCollection)) {
                messages.FindAndRemove(query, SortBy.Null);
            }
        }

        public void QueueMessage(Organization organization, Message message) {
            GetMessagesCollection(organization).Insert(message);
        }

        public Message GetMessage(Organization organization, User user, string id) {
            var message = GetMessagesCollection(organization).FindOneByIdAs<Message>(id);
            var contact = GetContact(message);
            if(contact != null && contact.ClaimedByUserId != user.Id) {
                return null;
            }
            return message;
        }

        public Message GetMessage(Organization organization, string id) {
            return GetMessagesCollection(organization).FindOneByIdAs<Message>(id);
        } 

        public IEnumerable<Message> GetMessages(Organization organization, Contact contact) {
            var queries = new List<IMongoQuery>();
            var identities = contact.GetSourceIdentities();
            foreach(var identity in identities) {
                var subQuery = Query.And(
                    Query.EQ("Source", identity.Key),
                    Query.EQ("Author._id", identity.Value.Id)
                    );
                queries.Add(subQuery);
            }
            var query = Query.Or(queries.ToArray());
            return GetMessagesCollection(organization).FindAs<Message>(query);
        }

        public void DeleteMessage(Organization organization, string id) {
            GetMessagesCollection(organization).Remove(Query.EQ("_id", id));
        }

        private MongoCollection GetMessagesCollection(Organization organization) {
            var messages = _db.GetCollection<Message>(organization.Id + "_messages");
            var indexes = new IndexKeysBuilder().Ascending("Source", "SourceMessageId");
            messages.EnsureIndex(indexes, IndexOptions.SetUnique(true));
            messages.EnsureIndex("MessageThreadIds");
            return messages;
        }
    }
}
