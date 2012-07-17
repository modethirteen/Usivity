using MongoDB.Bson;

namespace Usivity.Util {

    public class GuidGenerator : IGuidGenerator {

        //--- Methods ---
        public string GenerateNewObjectId() {
            return ObjectId.GenerateNewId().ToString();
        }   
    }
}
