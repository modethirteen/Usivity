using MongoDB.Bson.Serialization;
using Usivity.Entities;

namespace Usivity.Data.Serializers {

    public class UserSerializer : IUsivityDataSerializer {

        //--- Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(User))) {
                BsonClassMap.RegisterClassMap<User>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_organizations");
                });
            }
        }
    }
}
