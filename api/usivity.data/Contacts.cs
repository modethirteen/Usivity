using System.Collections.Generic;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public Contact GetContact(Message message) {
            if(message == null) {
                return null;
            }
            var identity = "_identities." + message.Source;
            var query = Query.And(
                Query.Exists(identity, true),
                Query.EQ(identity + "._id", message.Author.Id)
                );
            return _contacts.FindOneAs<Contact>(query);
        }

        public Contact GetContact(string id, User user) {
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("ClaimedByUserId", user.Id)
            );
            return _contacts.FindOneAs<Contact>(query);
        }

        public IEnumerable<Contact> GetContacts(User user) {
            var query = Query.EQ("ClaimedByUserId", user.Id);
            return _contacts.FindAs<Contact>(query);
        }

        public void SaveContact(Contact contact) {
            SaveEntity(_contacts, contact);
        }
    }
}
