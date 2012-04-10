using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class User : IEntity {

        //--- Constants ---
        public const string ANONYMOUS_USER = "Anonymous";

        //--- Class Properties ---
        public enum UserRole { Owner, Admin, Member, None }

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Password { get; set; }
        public string CurrentOrganization { get; set; }
        public bool IsAnonymous { get; private set; }

        //--- Fields ---
        private Dictionary<string, UserRole> _organizations;

        //--- Constructors ---
        public User(string name) {
            Id = UsivityDataSession.GenerateEntityId(this);
            Name = name;
            IsAnonymous = name == ANONYMOUS_USER;
            _organizations = new Dictionary<string, UserRole>();
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "user";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource).Attr("id", Id)
                .Elem("name", Name);
        }

        public UserRole GetOrganizationRole(Organization organization) {
            if(organization == null) {
                return UserRole.None;
            }
            return _organizations.TryGetValue(organization.Id, UserRole.None);
        }

        public void SetOrganizationRole(Organization organization, UserRole role) {
            _organizations[organization.Id] = role;
        }

        public IEnumerable<string> GetOrganizationIds() {
            return _organizations.Select(organization => organization.Key).ToList();
        }
    }
}
