using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.Twitter;
using Usivity.Util;

namespace Usivity.Services.Core.Logic {

    public class Connections : IConnections {

        //--- Fields ---
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganization _currentOrganization;
        private readonly IEmailClientFactory _emailClientFactory;
        private readonly ITwitterClientFactory _twitterClientFactory;

        //--- Constructors ---
        public Connections(
            IGuidGenerator guidGenerator,
            IUsivityDataCatalog data,
            ICurrentContext context,
            IOrganizations organizations,
            IEmailClientFactory emailClientFactory,
            ITwitterClientFactory twitterClientFactory
        ) {
            _guidGenerator = guidGenerator;
            _context = context;
            _data = data;
            _currentOrganization = organizations.CurrentOrganization;
            _emailClientFactory = emailClientFactory;
            _twitterClientFactory = twitterClientFactory;
        }

        //--- Methods ---
        public ITwitterConnection NewTwitterConnection() {
            var connection = new TwitterConnection(_guidGenerator, _currentOrganization);
            var client = _twitterClientFactory.NewTwitterClient(connection);
            connection.OAuthRequest = client.NewOAuthRequestToken();
            return connection;
        }

        public IEmailConnection NewEmailConnection() {
            return new EmailConnection(_guidGenerator, _currentOrganization); 
        }

        public void ActivateTwitterConnection(ITwitterConnection connection, TwitterAuthorization authorization) {
            if(authorization.OAuthRequestToken != connection.OAuthRequest.Token) {
                throw new Exception("OAuth request token mismatch");
            }
            var client = _twitterClientFactory.NewTwitterClient(connection);
            connection.OAuthRequest.Verifier = authorization.OAuthVerifier;
            Identity identity;
            var accessToken = client.NewOAuthAccessToken(connection.OAuthRequest, out identity);
            connection.Identity = identity;
            connection.OAuthAccess = accessToken;
            connection.OAuthRequest = null;
        }

        public void ActivateEmailConnection(IEmailConnection connection, EmailAuthorization authorization) {
            if(string.IsNullOrEmpty(authorization.Username) || string.IsNullOrEmpty(authorization.Host)) {
                throw new Exception("Invalid authorization. Required fields: host, username");
            }
            var testConnection = connection.Clone() as IEmailConnection;
            try {
                if(testConnection == null) {
                    throw new Exception("Could not create test email connection for validation");
                }
                testConnection.Host = authorization.Host;
                testConnection.Username = authorization.Username;
                testConnection.Password = authorization.Password;
                testConnection.Port = authorization.Port;
                testConnection.UseSsl = authorization.UseSsl;
                testConnection.UseCramMd5 = authorization.UseCramMd5;
                var emailClient = _emailClientFactory.NewEmailClient(testConnection);
                Exception clientErrorResponse;
                if(!emailClient.AreEmailConnectionCredentialsValid(out clientErrorResponse)) {
                    throw clientErrorResponse;
                }
            }
            catch(Exception e) {
                throw new Exception("Could not successfully validate email connection settings", e);
            }
            connection.Host = authorization.Host;
            connection.Username = authorization.Username;
            connection.Password = authorization.Password;
            connection.Port = authorization.Port;
            connection.UseSsl = authorization.UseSsl;
            connection.UseCramMd5 = authorization.UseCramMd5;
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
                connection = GetConnections(source).FirstOrDefault(c => c.Active);
            }
            return connection; 
        }

        public IConnection GetConnectionReceipient(IMessage message) {
            return GetConnections().FirstOrDefault(c =>
                c.Active && c.Identity.Id == message.SourceInReplyToIdentityId && c.Source == message.Source
                );         
        }

        public IEnumerable<IConnection> GetConnections(Source? source = null) {
            return _data.Connections.Get(_currentOrganization, source);
        }

        public XDoc GetConnectionXml(IConnection connection) {
            return connection.ToDocument().Attr("href", _context.ApiUri.At("connections", connection.Id));
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
