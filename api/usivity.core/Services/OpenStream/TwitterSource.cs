using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using Usivity.Core.Libraries.Json;

namespace Usivity.Core.Services.OpenStream {

    class TwitterSource : ISource {

        //--- Constants ---
        public const string SOURCE = "twitter";
        private const string URI = "http://search.twitter.com/search.json";

        //--- Properties ---
        public string Name { get; private set; }
        public IList<Subscription> Subscriptions { get; set; }

        //--- Constructors ---
        public TwitterSource() {
            Name = SOURCE;
            Subscriptions = new List<Subscription>();
        }

        //--- Methods ---
        public Uri GenerateUriWithConstraints(IList<string> constraints) {
            var query = "?q=" + string.Join(",", constraints.ToArray());
            return new Uri(URI + query);
        }

        public IList<Message> GetMessages() {
            var messages = new List<Message>();
            foreach (var subscription in Subscriptions) {
                if(!subscription.Active) {
                    return messages;
                }
                DreamMessage msg = Plug.New(subscription.Uri).Get();
                if(!msg.IsSuccessful) {
                    return messages;
                }
                var response = new JDoc(msg.ToText()).ToDocument("response");
                var nextPage = response["next_page"].Contents;
                var query = !string.IsNullOrEmpty(nextPage) ? nextPage : response["refresh_url"].Contents;
                subscription.Uri = new XUri(URI + query);

                var results = response["results"];
                messages.AddRange(results.Select(result => new Message() {
                    Source = SOURCE,
                    SourceId = result["id_str"].AsText,
                    Body = result["text"].AsText,
                    Author = new Author {
                        Name = result["from_user"].AsText,
                        Avatar = result["profile_image_url"].AsUri
                    },
                    Timestamp = DateTime.Parse(result["created_at"].AsText)
                }));
            }
            return messages;
        }
    }
}
