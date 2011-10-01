using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Usivity.Core.Services.OpenStream {

    class Subscription {

        //--- Properties ---
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Source { get; private set; }
        public IList<string> Constraints { get; set; }
        public Uri Uri { get; set; }
        public bool Active { get; set; }

        //--- Constructors ---
        public Subscription(string source) {
            Source = source;
            Active = true;
        }
    }
}
