using MongoDB.Bson.Serialization;
using Usivity.Entities;

namespace Usivity.Data.Serializers {

    public class OrganizationSerializer : IUsivityDataSerializer {

        //--- Class Methods ---
        public void RegisterSerializer() {
            if(!BsonClassMap.IsClassMapRegistered(typeof(Organization))) {
                BsonClassMap.RegisterClassMap<Organization>(cm => {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.MapField("_defaultConnections");
                });
            }
        }
    }
}
