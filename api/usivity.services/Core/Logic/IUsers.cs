using MindTouch.Xml;
using Usivity.Entities;

namespace Usivity.Services.Core.Logic {

    public interface IUsers {

        //--- Methods ---
        User GetUser(string id);
        User GetCurrentUser();
        User NewUser(string name, string password, string email);
        User GetAuthenticatedUser(string name, string password);
        XDoc GetUserXml(User user, string relation = null);
        XDoc GetUsersXml();
        XDoc GetCurrentUserXml();
        bool UsernameExists(string name);
        void SaveUser(User user);
        User.UserRole GetCurrentRole();
    }
}
