using MongoDB.Bson.Serialization;
using Usivity.Entities.Connections;

namespace Usivity.Data.Serializers {
   public class OAuthTokenInfoSerializer : IUsivityDataSerializer {

        //--- Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(OAuthTokenInfo))) {
                BsonClassMap.RegisterClassMap<OAuthTokenInfo>(cm => {
                    cm.AutoMap();
                    cm.MapIdField("_id");
                });
            }
        }
    }
}
