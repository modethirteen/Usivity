using System;
using System.Collections.Generic;
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
        public DateTime SourceCreated { get; set; }
        public string ParentMessageId { get; set; }
        public IList<string> MessageThreadIds { get; private set; }
        public Identity Author { get; set; }
        public string UserId { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public DateTime Created { get; private set; }
        public DateTime? Expires { get; private set; }
        public bool OpenStream { get; set; }

        //--- Constructors ---
        public Message(IGuidGenerator guidGenerator, IDateTime dateTime, TimeSpan? expiration = null) {
            Id = guidGenerator.GenerateNewObjectId();
            MessageThreadIds = new List<string>();
            Created = dateTime.UtcNow;
            OpenStream = true;
            if(expiration != null) {
                Expires = dateTime.UtcNow.Add(expiration.Value);    
            }
        }

        //--- Methods ---
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
