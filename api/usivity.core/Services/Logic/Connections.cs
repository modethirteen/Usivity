using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Connections;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services.Logic {

    public class Connections : IConnections {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly Organization _currentOrganization;

        //--- Constructors ---
        public Connections(IUsivityDataCatalog data, ICurrentContext context, IOrganizations organizations) {
            _context = context;
            _data = data;
            _currentOrganization = organizations.CurrentOrganization;
        }

        public IConnection GetConnection(string id) {
            return _data.Connections.Get(id);
        }

        public IConnection GetDefaultConnection(Source source) {
            IConnection connection = null;
            var connectionId = _currentOrganization.GetDefaultConnectionId(source);
            if(!string.IsNullOrEmpty(connectionId)) {
                connection = GetConnection(connectionId);
            }
            if(connection == null) {

                // if no default set get first matching source
                connection = GetConnections().FirstOrDefault(c => c.Source == source);
            }
            return connection; 
        }

        public IConnection GetConnectionReceipient(Message message) {
            return GetConnections().FirstOrDefault(c =>
                    c.Identity.Id == message.SourceInReplyToIdentityId && c.Source == message.Source
                    );         
        }

        public IEnumerable<IConnection> GetConnections() {
            return _data.Connections.Get(_currentOrganization);
        }

        public IEnumerable<IConnection> GetConnections(Source source) {
            return _data.Connections.Get(_currentOrganization, source);
        }

        public XDoc GetConnectionXml(IConnection connection) {
            return connection.ToDocument().Attr("href", _context.ApiUri.At("connections", connection.Id));
        }

        public XDoc GetConnectionsXml() {
            var connections = GetConnections();
            var doc = new XDoc("connections")
                .Attr("count", connections.Count())
                .Attr("href", _context.ApiUri.At("connections"));
            foreach(var connection in connections) {
                doc.Add(GetConnectionXml(connection));
            }
            doc.EndAll();
            return doc;
        }

        public void SaveConnection(IConnection connection) {
            _data.Connections.Save(connection);
        }

        public void DeleteConnection(IConnection connection) {
            _data.Connections.Delete(connection);
        }
    }
}
