using System;
using System.Globalization;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class Message : IEntity {

        //--- Class Properties ---
        public enum MessageStreams { Open, User }

        //--- Properties ---
        public string Id { get; private set; }
        public string Source { get; set; }
        public string SourceMessageId { get; set; }
        public string InReplyToId { get; set; }
        public SourceIdentity Author { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? Expires { get; private set; }
        public DateTime? NextAccess { get; set; }
        public MessageStreams Stream { get; set; }
        
        //--- Constructors ---
        public Message(MessageStreams stream) {
            Id = UsivityDataSession.GenerateEntityId(this);
            MoveToStream(stream);
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "message";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            var datetime = Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            return new XDoc(resource)
                .Attr("id", Id)
                .Elem("source", Source)
                .Start("author")
                    .Elem("name", Author.Name)
                    .Elem("uri.avatar", Author.Avatar.ToString())
                .End()
                .Elem("body", Body)
                .Elem("timestamp", datetime)
            .EndAll();
        }

        public void MoveToStream(MessageStreams stream) {
            if(stream == MessageStreams.Open) {
                Expires = DateTime.UtcNow.AddDays(4);
                NextAccess = DateTime.UtcNow;
            }
            Stream = stream;
        }
    }
}
