namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public User GetUser(string id) {
            return _users.FindOneByIdAs<User>(id);
        }

        public void SaveUser(User user) {
            SaveEntity(_users, user);
        }
    }
}
