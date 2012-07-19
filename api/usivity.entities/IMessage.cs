using System;
using System.Collections.Generic;
using Usivity.Entities.Types;

namespace Usivity.Entities {

    public interface IMessage : IEntity {
        
        //--- Properties ---
        string Id { get; }
        Source Source { get; set; }
        string SourceMessageId { get; set; }
        string SourceInReplyToMessageId { get; set; }
        string SourceInReplyToIdentityId { get; set; }
        DateTime SourceTimestamp { get; set; }
        string ParentMessageId { get; set; }
        IList<string> MessageThreadIds { get; }
        Identity Author { get; set; }
        string UserId { get; set; }
        string Body { get; set; }
        string Subject { get; set; }
        DateTime Timestamp { get; }
        DateTime? Expires { get; }
        bool OpenStream { get; set; }

        //--- Methods ---
        void SetParent(IMessage message);
        void RemoveExpiration();
    }
}
