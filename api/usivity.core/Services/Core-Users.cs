using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:users/authentication", "Get user authentication")]
        [DreamFeatureParam("redirect", "uri?", "Redirect to uri upon authentication")]
        public Yield GetUserAuthentication(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            User user;
            string authToken;
            if(request.Headers.Authorization != null) {
                string username, password;
                HttpUtil.GetAuthentication(context.Uri.ToUri(), request.Headers, out username, out password);
                password = _auth.GetSaltedPassword(password);
                user = _data.Users.GetAuthenticated(username, password);
                authToken = _auth.GenerateAuthToken(user);
            }
            else {
                authToken = _auth.GetAuthToken(request);
                user = _auth.GetAuthenticatedUser(authToken);
            }
            if(user == null) {
                response.Return(new DreamMessage(DreamStatus.Unauthorized, new DreamHeaders()));
                yield break;
            }

            var redirect = XUri.TryParse(context.GetParam("redirect", null));
            var expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(_authExpiration));
            var setCookie = _auth.GetAuthCookie(authToken, expires).ToSetCookieHeader();
            var auth = redirect != null ? DreamMessage.Redirect(redirect) : DreamMessage.Ok(MimeType.TEXT_UTF8, setCookie);
            auth.Headers["Set-Cookie"] = setCookie;
            response.Return(auth);
            yield break;
        }

        [DreamFeature("GET:users", "Get all users")]
        internal Yield GetUsers(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var users = _data.Users.Get(UsivityContext.Current.Organization);
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

        [DreamFeature("GET:users/current", "Get current user")]
        protected Yield GetCurrentUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = GetUserXml(UsivityContext.Current.User);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:users/{userid}", "Get user")]
        [DreamFeatureParam("userid", "string", "User id")]
        internal Yield GetUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var user = _data.Users.Get(context.GetParam<string>("userid"), UsivityContext.Current.Organization);
            if(user == null) {
                response.Return(DreamMessage.NotFound("The requested user could not be located"));
                yield break;
            }
            var doc = GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("POST:users", "Create a new user")]
        internal Yield PostUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var userDoc = GetRequestXml(request);
            var name = userDoc["name"].Contents;
            var password = userDoc["password"].Contents;

            if(string.IsNullOrEmpty(name)) {
                response.Return(DreamMessage.BadRequest("Request is missing a value for \"name\""));
                yield break;
            }
            if(name == User.ANONYMOUS_USER) {
                response.Return(DreamMessage.BadRequest("\"" + User.ANONYMOUS_USER + "\" is not a valid user name"));
                yield break;
            }
            if(string.IsNullOrEmpty(password)) {
                response.Return(DreamMessage.BadRequest("Request is missing a value for \"password\""));
                yield break;
            }
            if(_data.Users.Exists(name)) {
                response.Return(DreamMessage.Conflict("The requested user name has already been taken"));
            }
            var user = new User(name);
            user.SetOrganizationRole(UsivityContext.Current.Organization, User.UserRole.Member);
            user.CurrentOrganization = UsivityContext.Current.Organization.Id;
            user.Password = _auth.GetSaltedPassword(password);
            _data.Users.Save(user);

            var doc = GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("PUT:users/{userid}/password", "Change user password")]
        [DreamFeatureParam("userid", "string", "User id")]
        protected Yield UpdateUserPassword(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            if(context.GetParam<string>("userid") != UsivityContext.Current.User.Id) {
                response.Return(DreamMessage.Forbidden("You can only change your own user password"));
                yield break;
            }
            var password = request.ToText();
            UsivityContext.Current.User.Password = _auth.GetSaltedPassword(password);
            _data.Users.Save(UsivityContext.Current.User);
            response.Return(DreamMessage.Ok(MimeType.TEXT_UTF8, "Your password has been successfully updated"));
            yield break;
        }

        //--- Methods ---
        private XDoc GetUserXml(User user, string relation = null) {
            return user.ToDocument(relation).Attr("href", _usersUri.At(user.Id));
        }
    }
}
