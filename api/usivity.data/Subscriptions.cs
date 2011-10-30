using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public partial class UsivityDataSession {

        public IEnumerable<Subscription> GetSubscriptions(string source, User user = null) {
            var query = new QueryDocument {
                {"Source", source}
            };
            if(user != null) {
                query.Add("UserId", user.Id);
            }
            return _subscriptions.FindAs<Subscription>(query); 
        }

        public Subscription GetSubscription(string id) {
            return _subscriptions.FindOneByIdAs<Subscription>(id);
        }

        public void SaveSubscription(Subscription subscription) {
            SaveEntity(_subscriptions, subscription);
        }

        public void DeleteSubscription(Subscription subscription) {
            _subscriptions.Remove(Query.EQ("_id", subscription.Id));
        }
    }
}