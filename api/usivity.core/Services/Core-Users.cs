using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using Usivity.Core.Services.Logic;
using Usivity.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [UsivityFeatureAccess(User.UserRole.None)]
        [DreamFeature("GET:users/authentication", "Get user authentication")]
        [DreamFeatureParam("redirect", "uri?", "Redirect to uri upon authentication")]
        internal Yield GetUserAuthentication(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var users = Resolve<IUsers>(context);
            var auth = Resolve<IUsivityAuth>(context);
            User user;
            string authToken;
            if(request.Headers.Authorization != null) {
                string username, password;
                HttpUtil.GetAuthentication(context.Uri.ToUri(), request.Headers, out username, out password);
                password = auth.GetSaltedPassword(password);
                user = users.GetAuthenticatedUser(username, password);
                authToken = auth.GenerateAuthToken(user);
            }
            else {
                authToken = auth.GetAuthToken(request);
                user = auth.GetUser(authToken);
            }
            if(user == null || user.IsAnonymous) {
                response.Return(new DreamMessage(DreamStatus.Unauthorized, new DreamHeaders()));
                yield break;
            }
            var redirect = XUri.TryParse(context.GetParam("redirect", null));
            var setCookie = auth.GetAuthCookie(authToken, Self.Uri.AsPublicUri()).ToSetCookieHeader();
            var authResponse = redirect != null ? DreamMessage.Redirect(redirect) : DreamMessage.Ok(MimeType.TEXT_UTF8, setCookie);
            authResponse.Headers["Set-Cookie"] = setCookie;
            response.Return(authResponse);
            yield break;
        }
        
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:users", "Get all users")]
        internal Yield GetUsers(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IUsers>(context).GetUsersXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:users/current", "Get current user")]
        internal Yield GetCurrentUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IUsers>(context).GetCurrentUserXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }
        
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:users/{userid}", "Get user")]
        [DreamFeatureParam("userid", "string", "User id")]
        internal Yield GetUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var users = Resolve<IUsers>(context);
            var user = users.GetUser(context.GetParam<string>("userid"));
            if(user == null) {
                response.Return(DreamMessage.NotFound("The requested user could not be located"));
                yield break;
            }
            var doc = users.GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
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
            var users = Resolve<IUsers>(context);
            if(users.UsernameExists(name)) {
                response.Return(DreamMessage.Conflict("The requested user name has already been taken"));
            }
            var user = users.GetNewUser(name, password);
            users.SaveUser(user);
            var doc = users.GetUserXml(user);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("PUT:users/{userid}/password", "Change user password")]
        [DreamFeatureParam("userid", "string", "User id")]
        internal Yield UpdateUserPassword(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var user = Resolve<IUsers>(context).GetCurrentUser();
            if(context.GetParam<string>("userid") != user.Id) {
                response.Return(DreamMessage.Forbidden("You can only change your own user password"));
                yield break;
            }
            var users = Resolve<Users>(context);
            users.SavePassword(user, request.ToText());
            response.Return(DreamMessage.Ok(MimeType.TEXT_UTF8, "Your password has been successfully updated"));
            yield break;
        }
    }
}
