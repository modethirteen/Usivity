using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.Twitter;

namespace Usivity.Services.Core.Logic {

    public interface IConnections {

        //--- Methods ---
        ITwitterConnection NewTwitterConnection();
        IEmailConnection NewEmailConnection();
        void ActivateTwitterConnection(ITwitterConnection connection, TwitterAuthorization authorization);
        void ActivateEmailConnection(IEmailConnection connection, EmailAuthorization authorization);
        IConnection GetConnection(string id);
        IConnection GetDefaultConnection(Source source);
        IConnection GetConnectionReceipient(IMessage message);
        IEnumerable<IConnection> GetConnections(Source? source);
        XDoc GetConnectionXml(IConnection connection);
        XDoc GetConnectionsXml();
        void SaveConnection(IConnection connection);
        void DeleteConnection(IConnection connection);
    }
}
