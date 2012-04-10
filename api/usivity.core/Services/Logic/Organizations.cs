using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

    public class Organizations : IOrganizations {

        //--- Properties ---
        public Organization CurrentOrganization {
            get {
                if(_currentOrganization == null) {
                    _currentOrganization =_context.User != null
                        ? GetOrganization(_context.User.CurrentOrganization)
                        : Organization.NewMockOrganization();        
                }
                return _currentOrganization;
            }
        }

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataSession _data;
        private Organization _currentOrganization;

        //--- Constructors ---
        public Organizations(IUsivityDataSession data, ICurrentContext context) {
            _context = context;
            _data = data;
        }

        public Organization GetOrganization(string id) {
            return _data.Organizations.Get(id, _context.User); 
        }

        public XDoc GetOrganizationXml(Organization organization, string relation = null) {
            return organization.ToDocument(relation)
                .Attr("href", _context.ApiUri.At("organizations", organization.Id));
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
