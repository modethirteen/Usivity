using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;

namespace Usivity.Data {

    public class SubscriptionDataAccess : ISubscriptionDataAccess {
        
        //--- Class Methods ---
        public static void RegisterEntityClassMap() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(Subscription))) {
                BsonClassMap.RegisterClassMap<Subscription>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_uris");
                });
            }
        }   

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public SubscriptionDataAccess(MongoDatabase db) {
            _db = db.GetCollection<Subscription>("subscriptions");
        }

        //--- Methods ---
        public IEnumerable<Subscription> Get(IOrganization organization) {
            var query = Query.EQ("OrganizationId", organization.Id);
            return _db.FindAs<Subscription>(query); 
        }

        public Subscription Get(string id, IOrganization organization) {
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("OrganizationId", organization.Id)
                );
            return _db.FindOneAs<Subscription>(query);
        }

        public void Save(Subscription subscription) {
            _db.Save(subscription, SafeMode.True);
        }

        public void Delete(Subscription subscription) {
            _db.Remove(Query.EQ("_id", subscription.Id));
        }
    }
}