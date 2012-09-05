using System.Collections.Generic;
using MindTouch.OAuth;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;

namespace Usivity.Services.Core.Logic {

    public interface IConnections {

        //--- Methods ---
        XDoc GetOAuthRequestTokenXml(OAuthRequestToken token);
        OAuthRequestToken NewOAuthRequestToken(Source source);
        OAuthRequestToken GetOAuthRequestToken(Source source, string token);
        ITwitterConnection NewTwitterConnection(OAuthRequestToken token, string verifier);
        IEmailConnection NewEmailConnection(string host, string username, string password, int port, bool useSsl, bool useCramMd5);
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
