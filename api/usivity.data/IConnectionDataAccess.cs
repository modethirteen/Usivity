using System.Collections.Generic;
using Usivity.Connections;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Data {

    public interface IConnectionDataAccess {

        //--- Methods ---
        IEnumerable<IConnection> Get(IOrganization organization, Source? source = null);
        IConnection Get(string id);
        void Save(IConnection connection);
        void Delete(IConnection connection);
    }
}
