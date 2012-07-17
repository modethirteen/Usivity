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

        //--- Constructors ---
        public ConnectionDataAccess(MongoDatabase db) {
            _db = db.GetCollection("connections");
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
    }
}
