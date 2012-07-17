using MongoDB.Bson.Serialization;
using Usivity.Entities;

namespace Usivity.Data.Serializers {

    public class SubscriptionSerializer : IUsivityDataSerializer {

        //--- Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(Subscription))) {
                BsonClassMap.RegisterClassMap<Subscription>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_uris");
                });
            }
        }
    }
}
