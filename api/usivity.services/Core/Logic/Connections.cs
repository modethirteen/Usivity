using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Connections;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Services.Core.Logic {

    public class Connections : IConnections {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganization _currentOrganization;

        //--- Constructors ---
        public Connections(IUsivityDataCatalog data, ICurrentContext context, IOrganizations organizations) {
            _context = context;
            _data = data;
            _currentOrganization = organizations.CurrentOrganization;
        }

        public IConnection GetConnection(string id) {
            var connection = _data.Connections.Get(id);
            return connection.OrganizationId == _currentOrganization.Id ? connection : null;
        }

        public IConnection GetDefaultConnection(Source source) {
            IConnection connection = null;
            var connectionId = _currentOrganization.GetDefaultConnectionId(source);
            if(!string.IsNullOrEmpty(connectionId)) {
                connection = GetConnection(connectionId);
            }
            if(connection == null) {

                // if no default set get first matching active source
                connection = GetConnections(source).FirstOrDefault(c => c.Active);
            }
            return connection; 
        }

        public IConnection GetConnectionReceipient(Message message) {
            return GetConnections().FirstOrDefault(c =>
                c.Active && c.Identity.Id == message.SourceInReplyToIdentityId && c.Source == message.Source
                );         
        }

        public IEnumerable<IConnection> GetConnections(Source? source = null) {
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
