using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

    public interface IUsers {

        //--- Methods ---
        User GetUser(string id);
        User GetCurrentUser();
        User GetNewUser(string name, string password);
        User GetAuthenticatedUser(string name, string password);
        User GetAnonymousUser();
        XDoc GetUserXml(User user, string relation = null);
        XDoc GetUsersXml();
        XDoc GetCurrentUserXml();
        bool UsernameExists(string name);
        void SaveUser(User user);
        void SavePassword(User user, string password);
        User.UserRole GetCurrentRole();
    }
}
