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
        [DreamFeature("GET:users", "Get all users")]
        protected Yield GetUsers(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var users = _data.GetUsers(UsivityContext.Current.Organization);
            var doc = new XDoc("users")
                .Attr("count", users.Count())
                .Attr("href", _usersUri);
            foreach(var user in users) {
                doc.Add(GetUserXml(user));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:users/{userid}", "Get user")]
        [DreamFeatureParam("userid", "string", "User id")]
        protected Yield GetUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var user = _data.GetUser(context.GetParam<string>("contactid"), UsivityContext.Current.Organization);
            if(user == null) {
                response.Return(DreamMessage.NotFound("The requested user could not be located"));
                yield break;
            }
            var doc = GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
           
        }

        [DreamFeature("POST:users", "Create a new user")]
        protected Yield PostUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var user = _data.GetUser(context.GetParam<string>("contactid"), UsivityContext.Current.Organization);
            if(user == null) {
                response.Return(DreamMessage.NotFound("The requested user could not be located"));
                yield break;
            }
            var doc = GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
           
        }

        [DreamFeature("PUT:users/{userid}/password", "Change user password")]
        [DreamFeatureParam("userid", "string", "User id")]
        protected Yield UpdateUserPassword(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var user = _data.GetUser(context.GetParam<string>("contactid"), UsivityContext.Current.Organization);
            if(user == null) {
                response.Return(DreamMessage.NotFound("The requested user could not be located"));
                yield break;
            }
            var doc = GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
           
        }


        //--- Methods ---
        private XDoc GetUserXml(User user, string relation = null) {
            return user.ToDocument(relation).Attr("href", _usersUri.At(user.Id));
        }
    }
}
