using MongoDB.Bson;

namespace Usivity.Util {

    public class GuidGenerator {

        public static string CreateUnique() {
            return ObjectId.GenerateNewId().ToString();          
        }   
    }
}
