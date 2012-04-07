using System;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Data.Connections {

    public abstract class ATwitterConnection {

        //--- Constants ---
#if debug
        protected const string API_URI = "http://api.twitter.com/1";
#else
        protected const string API_URI = "https://api.twitter.com/1";
#endif

        //--- Methods ---
        virtual protected Message GetMessage(XDoc result) {
            XUri uri;
            XUri.TryParse(result["profile_image_url"].AsText, out uri);
            return new Message {
                Source = SourceType.Twitter,
                SourceMessageId = result["id_str"].AsText,
                SourceInReplyToMessageId = result["in_reply_to_status_id_str"].AsText,
                SourceInReplyToIdentityId = result["to_user_id_str"].AsText,
                Body = result["text"].AsText,
                Author = new Identity {
                    Id = result["from_user_id_str"].AsText,
                    Name = result["from_user"].AsText,
                    Avatar = uri != null ? uri.ToUri() : null
                },
                Timestamp = DateTime.Parse(result["created_at"].AsText)
            };
        }
    }
}
