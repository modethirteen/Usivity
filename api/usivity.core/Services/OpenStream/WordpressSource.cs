using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;

namespace Usivity.Core.Services.OpenStream {

    class WordpressSource : ISource {

        //--- Constants ---
        public const string SOURCE = "twitter";
        private const string URI = "http://search.twitter.com/search.json";

        //--- Fields ---
        private Plug _tweets;

        //--- Properties ---
        public string Id { get; private set; }
        public Uri Uri { get; private set; }

        //--- Constructors ---
        public TwitterSource(string query) {
            _tweets = Plug.New(URI).With("q", query);
            Uri = _tweets.Uri;
            Id = SOURCE;
        }

        public TwitterSource(Uri uri) {
            _tweets = Plug.New(uri);
            Uri = _tweets.Uri;
            Id = SOURCE;
        }

        //--- Methods ---
        public List<Message> GetMessages() {
            var messages = new List<Message>();
            DreamMessage msg = _tweets.Get();
            if(!msg.IsSuccessful) {
                return messages;
            }
            var response = new JDoc(msg.ToText()).ToDocument("response");
            var nextPage = response["next_page"].Contents;
            var query = !string.IsNullOrEmpty(nextPage) ? nextPage : response["refresh_url"].Contents;
            _tweets = Plug.New(URI + query);
            Uri = _tweets.Uri;

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
            return messages;
        }
    }
}
