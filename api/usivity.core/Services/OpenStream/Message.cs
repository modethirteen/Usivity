using System;
using System.Globalization;
using MindTouch.Xml;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Usivity.Core.Services.OpenStream {

    class Message {

        //--- Properties ---
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public string Source { get; set; }
        public string SourceId { get; set; }
        public Author Author { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime NextAccess { get; private set; }
        public DateTime Expires { get; private set; }
        public bool Active { get; set; }

        //--- Methods ---
        public XDoc ToDocument() {
            var datetime = Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            return new XDoc("message")
                .Elem("id", Id)
                .Elem("source", Source)
                .Start("author")
                    .Elem("name", Author.Name)
                    .Start("avatar")
                        .Attr("uri", Author.Avatar.ToString())
                    .End()
                .End()
                .Elem("body", Body)
                .Elem("timestamp", datetime)
            .EndAll();
        }

        //--- Constructors ---
        public Message() {
            NextAccess = DateTime.UtcNow;
            Expires = DateTime.UtcNow.AddDays(4);
            Active = true;
            Id = null;
        }
    }
}
