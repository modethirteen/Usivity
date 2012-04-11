using System;
using System.Collections.Generic;
using System.Globalization;
using MindTouch.Xml;
using Usivity.Data.Connections;

namespace Usivity.Data.Entities {

    public class Message : IEntity {

        //--- Properties ---
        public string Id { get; private set; }
        
        public SourceType Source { get; set; }
        public string SourceMessageId { get; set; }
        public string SourceInReplyToMessageId { get; set; }
        public string SourceInReplyToIdentityId { get; set; }

        public string ParentMessageId { get; set; }
        public IList<string> MessageThreadIds { get; private set; }
        public Identity Author { get; set; }
        public string UserId { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Expires { get; private set; }
        public bool OpenStream { get; set; }

        //--- Constructors ---
        public Message() {
            Id = UsivityDataSession.GenerateEntityId(this);
            MessageThreadIds = new List<string>();
            OpenStream = true;

            //TODO: make expiration datetime configurable
            Expires = DateTime.UtcNow.AddDays(4);
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "message";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            var datetime = Timestamp.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var author = new XDoc("author")
                .Elem("name", Author.Name ?? Author.Id);
            if(Author.Avatar != null) {
                author.Elem("uri.avatar", Author.Avatar.ToString());
            }
            return new XDoc(resource)
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .AddAll(author)
                .Elem("body", Body)
                .Elem("timestamp", datetime);
        }

        public void SetParent(Message message) {
            ParentMessageId = message.Id;
            MessageThreadIds = new List<string>();
            MessageThreadIds.AddRange(message.MessageThreadIds);
            MessageThreadIds.Add(message.Id);
        }
    }
}
