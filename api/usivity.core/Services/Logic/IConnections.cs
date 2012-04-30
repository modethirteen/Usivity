using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Connections;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services.Logic {

    public interface IConnections {

        //--- Methods ---
        IConnection GetConnection(string id);
        IConnection GetDefaultConnection(Source source);
        IConnection GetConnectionReceipient(Message message);
        IEnumerable<IConnection> GetConnections();
        IEnumerable<IConnection> GetConnections(Source source);
        XDoc GetConnectionXml(IConnection connection);
        XDoc GetConnectionsXml();
        void SaveConnection(IConnection connection);
        void DeleteConnection(IConnection connection);
    }
}
