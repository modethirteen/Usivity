using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

    public interface IOrganizations {

        //--- Properties ---
        Organization CurrentOrganization { get; }
        
        //--- Methods ---
        Organization GetOrganization(string id);
        XDoc GetOrganizationXml(Organization organization, string relation = null);
        XDoc GetOrganizationsXml();
    }
}
