using MindTouch.Dream;
using MindTouch.Web;
using Usivity.Entities;

namespace Usivity.Services.Core {

    public interface IUsivityAuth {

        //--- Methods ---
        string GetSaltedPassword(string password);
        string GetAuthToken(DreamMessage request);
        string GenerateAuthToken(User user);
        User GetUser(string authToken = null);
        DreamCookie GetAuthCookie(string authToken, XUri uri);
    }
}
