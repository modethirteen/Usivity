using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Core.Services.Logic;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:organizations", "Get organizations")]
        protected Yield GetOrganizations(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IOrganizations>(context).GetOrganizationsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:organizations/{organizationid}", "Get organization")]
        [DreamFeatureParam("organizationid", "string", "Organization id")]
        protected Yield GetOrganization(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organizations = Resolve<IOrganizations>(context);
            var organization = organizations.GetOrganization(context.GetParam<string>("organizationid"));
            if(organization == null) {
                response.Return(DreamMessage.NotFound("The requested organization could not be located"));
                yield break;
            }
            var doc = organizations.GetOrganizationXml(organization);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }
    }
}
