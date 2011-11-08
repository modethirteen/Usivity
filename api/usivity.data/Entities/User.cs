using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class User : IEntity {

        //--- Constants ---
        public const string ANONYMOUS_USER = "Anonymous";

        //--- Class Properties ---
        public enum UserRoles { Owner, Admin, Member, None }

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Password { get; set; }
        public string CurrentOrganizataion { get; set; }

        //--- Fields ---
        private Dictionary<string, IConnection> _connections;
        private Dictionary<string, UserRoles> _organizations;

        //--- Constructors ---
        public User(string name) {
            Id = UsivityDataSession.GenerateEntityId(this);
            Name = name;
            _organizations = new Dictionary<string, UserRoles>();
            _connections = new Dictionary<string, IConnection>();
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "user";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource).Attr("id", Id);
        }

        public IConnection GetConnection(string sourceId) {
            return _connections.TryGetValue(sourceId, null);
        }

        public void SetConnection(string sourceId, IConnection connection) {
            _connections[sourceId] = connection;
        }

        public UserRoles GetOrganizationRole(string organizationId) {
            return _organizations.TryGetValue(organizationId, UserRoles.None);
        }

        public void SetOrganizationRole(string organizationId, UserRoles role) {
            _organizations[organizationId] = role;
        }

        public IEnumerable<string> GetOrganizationIds() {
            return _organizations.Select(organization => organization.Key).ToList();
        }
    }
}
