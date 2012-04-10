using MindTouch.Xml;
using Usivity.Data.Connections;

namespace Usivity.Core.Services.Logic {

    public interface IConnections {

        //--- Methods ---
        IConnection GetConnection(string id);
        XDoc GetConnectionXml(IConnection connection);
        XDoc GetConnectionsXml();
        void SaveConnection(IConnection connection);
        void DeleteConnection(IConnection connection);
    }
}
