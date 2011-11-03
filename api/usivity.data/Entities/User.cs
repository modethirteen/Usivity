using System;
using System.Collections.Generic;
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
        public UserRoles Role { get; set; }
        public IEnumerable<string> Organizations { get; private set; }

        //--- Fields ---
        private Dictionary<string, IConnection> _connections;

        //--- Constructors ---
        public User(string name) {
            Id = UsivityDataSession.GenerateEntityId(this);
            if(name == ANONYMOUS_USER) {
                throw new ArgumentException("\"" + ANONYMOUS_USER + "\" is not a valid user name");
            }
            Name = name;
            Organizations = new List<string>();
            Role = UserRoles.Member;
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
    }
}
