using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

    public class Users : IUsers {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataSession _data;
        private readonly IOrganizations _organizations;
        private readonly IUsivityAuth _auth;

        //--- Constructors ---
        public Users(IUsivityDataSession data, ICurrentContext context, IOrganizations organizations, IUsivityAuth auth) {
            _context = context;
            _organizations = organizations;
            _data = data;
            _auth = auth;
        }

        //--- Methods ---
        public User GetUser(string id) {
            return _data.Users.Get(id, _organizations.CurrentOrganization);
        }

        public User GetCurrentUser() {
            return _context.User;
        }

        public User GetAnonymousUser() {
            return _data.Users.GetAnonymous();
        }

        public User GetAuthenticatedUser(string name, string password) {
            return _data.Users.GetAuthenticated(name, password);
        }

        public XDoc GetUserXml(User user, string relation = null) {
            return user.ToDocument(relation).Attr("href", _context.ApiUri.At("users", user.Id));
        }

        public XDoc GetUsersXml() {
            var users = _data.Users.Get(_organizations.CurrentOrganization);
            var doc = new XDoc("users")
                .Attr("count", users.Count())
                .Attr("href", _context.ApiUri.At("users"));
            foreach(var user in users) {
                doc.Add(GetUserXml(user));
            }
            doc.EndAll();
            return doc;
        }

        public XDoc GetCurrentUserXml() {
            return GetUserXml(_context.User);
        }

        public bool UsernameExists(string name) {
            return _data.Users.Exists(name);
        }

        public bool IsCurrentUserId(string id) {
            return _context.User.Id == id;
        }

        public User GetNewUser(string name, string password) {
            var user = new User(name);
            var organization = _organizations.CurrentOrganization;
            user.SetOrganizationRole(organization, User.UserRole.Member);
            user.CurrentOrganization = organization.Id;
            user.Password = _auth.GetSaltedPassword(password);
            return user;
        }

        public void SaveUser(User user) {
            _data.Users.Save(user);
        }

        public void SavePassword(User user, string password) {
            user.Password = _auth.GetSaltedPassword(password);
            _data.Users.Save(user);
        }

        public User.UserRole GetCurrentRole() {
            return _context.User.GetOrganizationRole(_organizations.CurrentOrganization);
        }
    }
}
