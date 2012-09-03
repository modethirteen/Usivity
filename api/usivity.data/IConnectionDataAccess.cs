using System.Collections.Generic;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;

namespace Usivity.Data {

    public interface IConnectionDataAccess {

        //--- Methods ---
        IEnumerable<IConnection> Get(IOrganization organization, Source? source = null);
        IConnection Get(string id);
        void Save(IConnection connection);
        void Delete(IConnection connection);

        // temporary token storage
        void StashTokenInfo(OAuthTokenInfo token);
        OAuthTokenInfo FetchTokenInfo(string token, Source source, IOrganization organization, IUser user);
    }
}
