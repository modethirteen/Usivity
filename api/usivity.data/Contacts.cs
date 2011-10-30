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

        public void SaveContact(Contact contact) {
            SaveEntity(_contacts, contact);
        }
    }
}
