using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:organizations", "Get organizations")]
        protected Yield GetOrganizations(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organizations = _data.Organizations.Get(UsivityContext.Current.User);
            var doc = new XDoc("organizations")
                .Attr("count", organizations.Count())
                .Attr("href", _organizationsUri);
            foreach(var organization in organizations) {
                doc.Add(GetOrganizationXml(organization));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:organizations/{organizationid}", "Get organization")]
        [DreamFeatureParam("organizationid", "string", "Organization id")]
        protected Yield GetOrganization(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = _data.Organizations
                .Get(context.GetParam<string>("organizationid"), UsivityContext.Current.User);
            if(organization == null) {
                response.Return(DreamMessage.NotFound("The requested organization could not be located"));
                yield break;
            }
            var doc = GetOrganizationXml(organization);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        //--- Methods ---
        private XDoc GetOrganizationXml(Organization organization, string relation = null) {
            return organization.ToDocument(relation)
                .Attr("href", _organizationsUri.At(organization.Id));
        }
    }
}
