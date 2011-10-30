using System;
using System.Collections.Generic;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class User : IEntity {

        //--- Properties ---
        public string Id { get; private set; }

        //--- Fields ---
        private Dictionary<string, IConnection> _connections;

        //--- Constructors ---
        public User () {
            Id = UsivityDataSession.GenerateEntityId(this);
            _connections = new Dictionary<string, IConnection>();
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "user";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc("user").Attr("id", Id);
        }

        public IConnection GetConnection(string sourceId) {
            return _connections.TryGetValue(sourceId, null);
        }

        public void SetConnection(string sourceId, IConnection connection) {
            _connections[sourceId] = connection;
        }
    }
}
