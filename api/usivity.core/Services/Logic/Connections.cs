using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Data.Connections;

namespace Usivity.Core.Services.Logic {

    public class Connections : IConnections {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataSession _data;
        private readonly IOrganizations _organizations;

        //--- Constructors ---
        public Connections(IUsivityDataSession data, ICurrentContext context, IOrganizations organizations) {
            _context = context;
            _data = data;
            _organizations = organizations;
        }

        public IConnection GetConnection(string id) {
            return _organizations.CurrentOrganization.GetConnection(id);
        }

        public XDoc GetConnectionXml(IConnection connection) {
            return connection.ToDocument().Attr("href", _context.ApiUri.At("connections", connection.Id));
        }

        public XDoc GetConnectionsXml() {
            var connections = _organizations.CurrentOrganization.Connections;
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
            var organization = _organizations.CurrentOrganization;
            organization.SetConnection(connection);
            _data.Organizations.Save(organization);
        }

        public void DeleteConnection(IConnection connection) {
            var organization = _organizations.CurrentOrganization;
            organization.RemoveConnection(connection.Id);
            _data.Organizations.Save(organization);
        }
    }
}
