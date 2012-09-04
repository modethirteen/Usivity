using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;

namespace Usivity.Services.Core.Logic {

    public class Organizations : IOrganizations {

        //--- Properties ---
        public IOrganization CurrentOrganization {
            get {
                if(_currentOrganization == null) {
                    _currentOrganization = GetOrganization(_context.User.CurrentOrganization);
                }
                return _currentOrganization;
            }
        }

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private IOrganization _currentOrganization;

        //--- Constructors ---
        public Organizations(IUsivityDataCatalog data, ICurrentContext context) {
            _context = context;
            _data = data;
        }

        public IOrganization GetOrganization(string id) {
            return _data.Organizations.Get(id, _context.User); 
        }

        public XDoc GetOrganizationXml(IOrganization organization, string relation = null) {
            var resource = "organization";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", organization.Id)
                .Attr("href", _context.ApiUri.At("organizations", organization.Id))
                .Elem("name", organization.Name);
        }

        public XDoc GetOrganizationsXml() {
            var organizations = _data.Organizations.Get(_context.User);
            var doc = new XDoc("organizations")
                .Attr("count", organizations.Count())
                .Attr("href", _context.ApiUri.At("organizations"));
            foreach(var organization in organizations) {
                doc.Add(GetOrganizationXml(organization));
            }
            doc.EndAll();
            return doc;
        }
    }
}
