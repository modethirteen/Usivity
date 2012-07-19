using MongoDB.Bson.Serialization;
using Usivity.Entities.Connections;

namespace Usivity.Data.Serializers {

    public class EmailConnectionSerializer : IUsivityDataSerializer {

        //--- Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(EmailConnection))) {
                BsonClassMap.RegisterClassMap<EmailConnection>();
            }
        }
    }
}
