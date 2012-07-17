using MongoDB.Bson.Serialization;
using Usivity.Entities;

namespace Usivity.Data.Serializers {

    public class ContactSerializer : IUsivityDataSerializer {
        
        //--- Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(Contact))) {
                BsonClassMap.RegisterClassMap<Contact>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_identities");
                    cm.MapField("_organizations");
                });
            }
        }
    }
}
