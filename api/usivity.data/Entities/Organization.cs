using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Data.Connections;

namespace Usivity.Data.Entities {

    public class Organization : IEntity {

        //--- Class Methods ---
        public static Organization NewMockOrganization() {
            return new Organization(string.Empty, string.Empty); 
        }

        public static Organization NewOrganization(string name) {
            return new Organization(name);
        }

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; private set; }
        public IEnumerable<IConnection> Connections { get { return _connections.Values; } }

        //--- Fields ---
        private IDictionary<string, IConnection> _connections;
        private IDictionary<SourceType, string> _defaultConnections;

        //--- Constructors ---
        private Organization(string name, string id = null) {
            Name = name;
            Id = id ?? UsivityDataSession.GenerateEntityId(this);
            _connections = new Dictionary<string, IConnection>();
            _defaultConnections = new Dictionary<SourceType, string>();
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "organization";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource).Attr("id", Id).Elem("name", Name);
        }

        public void SetConnection(IConnection connection) {
            _connections[connection.Id] = connection;
        }

        public IConnection GetConnection(string id) {
            return _connections.ContainsKey(id) ? _connections[id] : null;
        }

        public IConnection GetDefaultConnection(SourceType source) {
            string connectionId;
            IConnection connection = null;
            _defaultConnections.TryGetValue(source, out connectionId);
            if(!string.IsNullOrEmpty(connectionId)) {
                _connections.TryGetValue(connectionId, out connection); 
            }
            if(connection == null) {
                
                // if no default set get first matching source
                connection = _connections.Values.FirstOrDefault(c => c.Source == source);    
            }
            return connection;
        }

        public IConnection GetConnectionReceipient(Message message) {
            var connections = _connections.Values;
            return connections.FirstOrDefault(c =>
                c.Identity.Id == message.SourceInReplyToIdentityId && c.Source == message.Source
                );     
        }

        public IEnumerable<IConnection> GetConnectionsBySource(SourceType source) {
            var connections = new List<IConnection>();
            foreach(var connection in connections.Where(connection => connection.Source == source)) {
                connections.Add(connection);
            }
            return connections;
        }

        public void RemoveConnection(string id) {
            if(_connections.ContainsKey(id)) {
                _connections.Remove(id);    
            }
        }
    }
}
