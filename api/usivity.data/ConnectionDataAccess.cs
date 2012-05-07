using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Connections;
using Usivity.Connections.Email;
using Usivity.Connections.Twitter;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Data {

    public class ConnectionDataAccess : IConnectionDataAccess {

        //--- Class Methods ---
        public static void RegisterConnectionClassMaps() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(TwitterConnection))) {
                BsonClassMap.RegisterClassMap<TwitterConnection>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_oauth");
                    cm.MapField("_oauthRequest");
                });
            }
            if(!BsonClassMap.IsClassMapRegistered(typeof(EmailConnection))) {
                BsonClassMap.RegisterClassMap<EmailConnection>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_imapConfig");
                    cm.MapField("_smtpConfig");
                });
            }
        }

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
