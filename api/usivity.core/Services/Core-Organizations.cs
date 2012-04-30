using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Core.Services.Logic;
using Usivity.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:organizations", "Get organizations")]
        internal Yield GetOrganizations(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IOrganizations>(context).GetOrganizationsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:organizations/{organizationid}", "Get organization")]
        [DreamFeatureParam("organizationid", "string", "Organization id")]
        internal Yield GetOrganization(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
