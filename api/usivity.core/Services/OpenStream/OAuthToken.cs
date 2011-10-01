using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Usivity.Core.Services.OpenStream {

    class OAuthToken {
        
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
    }
}
