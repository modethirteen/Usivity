using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
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

        //--- Methods ---
        private XDoc GetUserXml(User user, string relation = null) {
            return user.ToDocument(relation).Attr("href", _usersUri.At(user.Id));
        }

        private User GetUserFromAuthToken(DreamMessage request) {
            string authToken = null;
            if(request.HasCookies) {
                var authCookie = DreamCookie.GetCookie(request.Cookies, AUTHTOKEN_COOKIENAME);
                if(authCookie != null && !authCookie.Expired) {
                    authToken = authCookie.Value;
                }
            }
            if(string.IsNullOrEmpty(authToken)) {
                authToken = request.Headers[AUTHTOKEN_HEADERNAME];    
            }
            if(authToken == null) {
                return null;
            }
            var m = _authTokenRegex.Match(authToken);
            if(!m.Success) {
                return null;
            }
            return null;
        }

        private string GenerateAuthToken(User user) {
            var token = string.Format("{0}_{1}", user.Id, DateTime.UtcNow.ToUniversalTime().Ticks);
            var validate = string.Format("{0}.{1}.{2}", token, user.Password ?? string.Empty, _securitySalt);
            var md5 = MD5.Create();
            var hash = new Guid(md5.ComputeHash(Encoding.Default.GetBytes(validate))).ToString("N");
            return token + "_" + hash;
        }
    }
}
