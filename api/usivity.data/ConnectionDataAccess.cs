using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;

namespace Usivity.Data {

    public class ConnectionDataAccess : IConnectionDataAccess {

        //--- Fields ---
        private readonly MongoCollection _db;

        // temporary collection until token cache gets moved to memcache/redis
        private readonly MongoCollection _tokenDb;

        //--- Constructors ---
        public ConnectionDataAccess(MongoDatabase db) {
            _db = db.GetCollection("connections");
            _tokenDb = db.GetCollection("tokens");

            // set ttl to 15 minutes
            _tokenDb.EnsureIndex(new IndexKeysBuilder().Ascending("Token.Created"),
                IndexOptions.SetTimeToLive(TimeSpan.FromMinutes(15)));
            var indexes = new IndexKeysBuilder().Ascending("Source", "OrganizationId", "UserId");
            _tokenDb.EnsureIndex(indexes, IndexOptions.SetUnique(true));
        }
   
        //--- Methods ---
        public IEnumerable<IConnection> Get(IOrganization organization, Source? source = null) {
            var query = Query.EQ("OrganizationId", organization.Id);
            if(source != null) {
                query = Query.And(query, Query.EQ("Source", source));
            }
            return _db.FindAs<IConnection>(query);
        }

        public IConnection Get(string id) {
            return _db.FindOneByIdAs<IConnection>(id);    
        }

        public void Save(IConnection connection) {
            _db.Save(connection, SafeMode.True);
        }

        public void Delete(IConnection connection) {
            _db.Remove(Query.EQ("_id", connection.Id));
        }

#region temporary token storage
        public void StashTokenInfo(OAuthTokenInfo token) {
            var query = Query.And(
                Query.EQ("Source", token.Source.GetSourceValue()),
                Query.EQ("OrganizationId", token.OrganizationId),
                Query.EQ("UserId", token.UserId)
                );
            _tokenDb.FindAndRemove(query, SortBy.Null);
            _tokenDb.Save(token);
        }

        public OAuthTokenInfo FetchTokenInfo(string token, Source source, IOrganization organization, IUser user) {
            var query = Query.And(
                Query.EQ("Token.Token", token),
                Query.EQ("Source", source.GetSourceValue()),
                Query.EQ("OrganizationId", organization.Id),
                Query.EQ("UserId", user.Id)
                );
            var info = _tokenDb.FindOneAs<OAuthTokenInfo>(query);
            if(info != null) {
                _tokenDb.Remove(query);
            }
            return info;
        }
    }
#endregion
}
