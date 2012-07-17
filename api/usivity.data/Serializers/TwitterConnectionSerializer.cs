using MongoDB.Bson.Serialization;
using Usivity.Entities.Connections;

namespace Usivity.Data.Serializers {

    public class TwitterConnectionSerializer : IUsivityDataSerializer {

        //--- Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(TwitterConnection))) {
                BsonClassMap.RegisterClassMap<TwitterConnection>();
            }
        }
    }
}
