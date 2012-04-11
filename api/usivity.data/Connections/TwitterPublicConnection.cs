using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data.Entities;
using Usivity.Util.Json;

namespace Usivity.Data.Connections {

    public class TwitterPublicConnection : ATwitterConnection, IPublicConnection {

        //--- Constants --
        protected const string SEARCH_URI = "http://search.twitter.com/search.json";

        //--- Class Properties ---
        public static readonly IDictionary<Subscription.SubscriptionLanguage, string> SubscriptionLanguagesMap = 
            new Dictionary<Subscription.SubscriptionLanguage, string> {
                { Subscription.SubscriptionLanguage.English, "en" }
        };

        //--- Class Fields ---
        private static TwitterPublicConnection _instance;

        //--- Class Methods ---
        public static TwitterPublicConnection GetInstance() {
            return _instance ?? (_instance = new TwitterPublicConnection());
        }

        //--- Constructors ---
        private TwitterPublicConnection() {}

        //--- Methods ---
        public IEnumerable<Message> GetMessages(Subscription subscription) {
            var messages = new List<Message>();
            if(!subscription.Active) {
                return messages;
            }
            DreamMessage msg = Plug.New(subscription.GetSourceUri(SourceType.Twitter)).Get();
            if(!msg.IsSuccessful) {
                return messages;
            }
            var response = new JDoc(msg.ToText()).ToDocument("response");
            var nextPage = response["next_page"].Contents;
            var query = !string.IsNullOrEmpty(nextPage) ? nextPage : response["refresh_url"].Contents;
            subscription.SetSourceUri(SourceType.Twitter, new XUri(SEARCH_URI + query));
            var results = response["results"];
            messages.AddRange(results.Select(GetMessage));
            return messages;
        }

        public void SetNewSubscriptionUri(Subscription subscription) {
            string lang;
            SubscriptionLanguagesMap.TryGetValue(subscription.Language, out lang);
            var uri = new XUri(SEARCH_URI).With("q", string.Join(",", subscription.Constraints.ToArray()));
            if(!string.IsNullOrEmpty(lang)) {
                uri.With("lang", lang);
            }
            subscription.SetSourceUri(SourceType.Twitter, uri);
        }

        protected override Message GetMessage(XDoc result) {
            var message = base.GetMessage(result);
            message.OpenStream = true;
            return message;
        }
    }
}
