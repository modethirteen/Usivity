using System;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Connections.Twitter {

    public abstract class TwitterConnectionBase {

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
                Source = Source.Twitter,
                SourceMessageId = result["id_str"].AsText,
                SourceInReplyToMessageId = result["in_reply_to_status_id_str"].AsText,
                SourceInReplyToIdentityId = result["to_user_id_str"].AsText,
                Body = result["text"].AsText,
                Author = new Identity {
                    Id = result["from_user_id_str"].AsText,
                    Name = result["from_user"].AsText,
                    Avatar = uri != null ? uri.ToUri() : null
                },
                SourceTimestamp = DateTime.Parse(result["created_at"].AsText)
            };
        }

        virtual protected Identity GetIdentityByUserId(string userId) {
            var msg = Plug.New(API_URI).At("users", "lookup.xml")
                .With("user_id", userId).Get();
            return ParseUserLookupResult(msg);
        }

        virtual protected Identity GetIdentityByScreenName(string screenName) {
             var msg = Plug.New(API_URI).At("users", "lookup.xml")
                .With("screen_name", screenName).Get();
            return ParseUserLookupResult(msg);
        }

        protected Identity ParseUserLookupResult(DreamMessage result) {
            var userDoc = result.ToDocument();
            if(!userDoc["error"].IsEmpty) {
                return null;
            }
            XUri uri;
            XUri.TryParse(userDoc["user/profile_image_url"].AsText, out uri);
            return new Identity {
                Id = userDoc["user/id"].AsText,
                Name = userDoc["user/screen_name"].AsText,
                Avatar = uri != null ? uri.ToUri() : null
            };
        }
    }
}
