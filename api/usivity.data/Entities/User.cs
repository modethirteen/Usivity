using System;
using System.Collections.Generic;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class User : IEntity {

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Password { get; set; }
        public IEnumerable<string> Organizations { get; private set; }

        //--- Fields ---
        private Dictionary<string, IConnection> _connections;

        //--- Constructors ---
        public User(string name) {
            Id = UsivityDataSession.GenerateEntityId(this);
            Name = name;
            Organizations = new List<string>();
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
