using System;
using System.Collections.Generic;
using System.Globalization;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities {

    public class Message : IMessage {

        //--- Properties ---
        public string Id { get; private set; }
        public Source Source { get; set; }
        public string SourceMessageId { get; set; }
        public string SourceInReplyToMessageId { get; set; }
        public string SourceInReplyToIdentityId { get; set; }
        public DateTime SourceTimestamp { get; set; }
        public string ParentMessageId { get; set; }
        public IList<string> MessageThreadIds { get; private set; }
        public Identity Author { get; set; }
        public string UserId { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public DateTime Timestamp { get; private set; }
        public DateTime? Expires { get; private set; }
        public bool OpenStream { get; set; }

        //--- Constructors ---
        public Message(IGuidGenerator guidGenerator, IDateTime dateTime, TimeSpan? expiration = null) {
            Id = guidGenerator.GenerateNewObjectId();
            MessageThreadIds = new List<string>();
            Timestamp = dateTime.UtcNow;
            OpenStream = true;
            if(expiration != null) {
                Expires = dateTime.UtcNow.Add(expiration.Value);    
            }
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "message";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            var datetime = SourceTimestamp.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var author = new XDoc("author")
                .Elem("name", Author.Name ?? Author.Id);
            if(Author.Avatar != null) {
                author.Elem("uri.avatar", Author.Avatar.ToString());
            }
            return new XDoc(resource)
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .AddAll(author)
                .Elem("subject", Subject)
                .Elem("body", Body)
                .Elem("timestamp", datetime);
        }

        public void SetParent(IMessage message) {
            ParentMessageId = message.Id;
            MessageThreadIds = new List<string>();
            MessageThreadIds.AddRange(message.MessageThreadIds);
            MessageThreadIds.Add(message.Id);
        }

        public void RemoveExpiration() {
            Expires = null;
        }
    }
}
