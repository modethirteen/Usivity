using MindTouch.Xml;
using Usivity.Entities;

namespace Usivity.Services.Core.Logic {

    public interface IOrganizations {

        //--- Properties ---
        IOrganization CurrentOrganization { get; }
        
        //--- Methods ---
        IOrganization GetOrganization(string id);
        XDoc GetOrganizationXml(IOrganization organization, string relation = null);
        XDoc GetOrganizationsXml();
    }
}
