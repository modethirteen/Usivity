using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Util;

namespace Usivity.Services.Core.Logic {

    public class Users : IUsers {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganizations _organizations;
        private readonly IUsivityAuth _auth;
        private readonly IGuidGenerator _guidGenerator;

        //--- Constructors ---
        public Users(IGuidGenerator guidGenerator, IUsivityDataCatalog data, ICurrentContext context, IOrganizations organizations, IUsivityAuth auth) {
            _context = context;
            _organizations = organizations;
            _data = data;
            _auth = auth;
            _guidGenerator = guidGenerator;
        }

        //--- Methods ---
        public User GetUser(string id) {
            return _data.Users.Get(id, _organizations.CurrentOrganization);
        }

        public User GetCurrentUser() {
            return _context.User;
        }

        public User GetAuthenticatedUser(string name, string password) {
            return _data.Users.GetAuthenticated(name, password);
        }

        public XDoc GetUserXml(User user, string relation = null) {
            var resource = "user";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", user.Id)
                .Attr("href", _context.ApiUri.At("users", user.Id))
                .Elem("name", user.Name)
                .Elem("email", user.EmailAddress)
                .Elem("role", user.GetOrganizationRole(_organizations.CurrentOrganization));
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

        public User NewUser(string name, string password, string email) {
            var user = new User(_guidGenerator, name, email);
            var organization = _organizations.CurrentOrganization;
            user.SetOrganizationRole(organization, User.UserRole.Member);
            user.CurrentOrganization = organization.Id;
            user.Password = _auth.GetSaltedPassword(password);
            return user;
        }

        public void SaveUser(User user) {
            _data.Users.Save(user);
        }

        public User.UserRole GetCurrentRole() {
            return _context.User.GetOrganizationRole(_organizations.CurrentOrganization);
        }
    }
}
