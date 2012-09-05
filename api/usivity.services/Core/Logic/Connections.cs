using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.OAuth;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.OAuth;
using Usivity.Services.Clients.Twitter;
using Usivity.Util;

namespace Usivity.Services.Core.Logic {

    public class Connections : IConnections {

        //--- Fields ---
        private readonly IDateTime _dateTime;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganization _currentOrganization;
        private readonly IEmailClientFactory _emailClientFactory;
        private readonly ITwitterClientFactory _twitterClientFactory;

        //--- Constructors ---
        public Connections(
            IGuidGenerator guidGenerator,
            IDateTime dateTime,
            IUsivityDataCatalog data,
            ICurrentContext context,
            IOrganizations organizations,
            IEmailClientFactory emailClientFactory,
            ITwitterClientFactory twitterClientFactory
        ) {
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
            _context = context;
            _data = data;
            _currentOrganization = organizations.CurrentOrganization;
            _emailClientFactory = emailClientFactory;
            _twitterClientFactory = twitterClientFactory;
        }

        //--- Methods ---
        public XDoc GetOAuthRequestTokenXml(OAuthRequestToken token) {
            return new XDoc("token")
                .Elem("uri.authorize", token.AuthorizeUri.ToString());
        }

        public OAuthRequestToken NewOAuthRequestToken(Source source) {
            IOAuthAccessClient client = null;
            switch(source) {
                case Source.Twitter:
                    client = _twitterClientFactory.NewTwitterOAuthAccessClient();
                    break;
                default:
                    throw new NotSupportedException();
            }

            // callback includes source to assist ui determining which endpoint to post oauth response to
            var callback = _context.UiUri.With("source", source.ToString().ToLowerInvariant());
            var token = client.NewOAuthRequestToken(callback);
            var info = new OAuthTokenInfo(token, source, _guidGenerator, _currentOrganization, _context.User);
            _data.Connections.StashTokenInfo(info);
            return token;
        }

        public OAuthRequestToken GetOAuthRequestToken(Source source, string token) {
            var info = _data.Connections.FetchTokenInfo(token, source, _currentOrganization, _context.User);
            return (info != null) ? info.Token : null;
        }

        public ITwitterConnection NewTwitterConnection(OAuthRequestToken token, string verifier) {
            token.Verifier = verifier;
            var access = _twitterClientFactory.NewTwitterOAuthAccessClient().NewOAuthAccessToken(token);
            var connection = new TwitterConnection(_guidGenerator, _currentOrganization, _dateTime) {
                OAuthAccess = access.Token,
                Identity = TwitterClient.NewIdentityFromUserId(access.Identity.Id)
            };
            return connection;
        }

        public IEmailConnection NewEmailConnection(string host, string username, string password, int port, bool useSsl, bool useCramMd5) {
            var connection = new EmailConnection(_guidGenerator, _currentOrganization, _dateTime) {
                Host = host,
                Username = username,
                Password = password,
                Port = port,
                UseSsl = useSsl,
                UseCramMd5 = useCramMd5
            };
            EmailClient.CheckEmailConnectionCredentials(connection);
            var id = username.Contains("@") ? username : string.Format("{0}@{1}", username, connection.Host);
            var identity = new Identity { Id = id, Name = id };
            connection.Identity = identity;
            return connection;
        }

        public IConnection GetConnection(string id) {
            var connection = _data.Connections.Get(id);
            return connection == null || connection.OrganizationId != _currentOrganization.Id ? null : connection;
        }

        public IConnection GetDefaultConnection(Source source) {
            IConnection connection = null;
            var connectionId = _currentOrganization.GetDefaultConnectionId(source);
            if(!string.IsNullOrEmpty(connectionId)) {
                connection = GetConnection(connectionId);
            }
            if(connection == null) {

                // if no default set get first matching active source
                connection = GetConnections(source).FirstOrDefault();
            }
            return connection; 
        }

        public IConnection GetConnectionReceipient(IMessage message) {
            return GetConnections().FirstOrDefault(c =>
                c.Identity.Id == message.SourceInReplyToIdentityId && c.Source == message.Source
                );         
        }

        public IEnumerable<IConnection> GetConnections(Source? source = null) {
            return _data.Connections.Get(_currentOrganization, source);
        }

        public XDoc GetConnectionXml(IConnection connection) {
            return new XDoc("connection")
                .Attr("id", connection.Id)
                .Attr("href", _context.ApiUri.At("connections", connection.Id))
                .Elem("source", connection.Source.ToString().ToLowerInvariant())
                .Start("identity")
                    .Attr("id", connection.Identity.Id)
                    .Elem("name", connection.Identity.Name)
                    .Elem("uri.avatar", connection.Identity.Avatar)
                .End()
                .Elem("created", connection.Created.ToISO8601String());
        }

        public XDoc GetConnectionsXml() {
            var connections = GetConnections();
            var doc = new XDoc("connections")
                .Attr("count", connections.Count())
                .Attr("href", _context.ApiUri.At("connections"));
            foreach(var connection in connections) {
                doc.Add(GetConnectionXml(connection));
            }
            doc.EndAll();
            return doc;
        }

        public void SaveConnection(IConnection connection) {
            _data.Connections.Save(connection);
        }

        public void DeleteConnection(IConnection connection) {
            _data.Connections.Delete(connection);
        }
    }
}
