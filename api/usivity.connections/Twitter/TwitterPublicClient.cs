using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Util;
using Usivity.Util.Json;

namespace Usivity.Connections.Twitter {

    public class TwitterPublicConnection : TwitterConnectionBase, IPublicConnection {

        //--- Constants --
        protected const string SEARCH_URI = "http://search.twitter.com/search.json";

        //--- Class Fields ---
        private static TwitterPublicConnection _instance;
        private static readonly IDictionary<Subscription.SubscriptionLanguage, string> _subscriptionLanguagesMap = 
            new Dictionary<Subscription.SubscriptionLanguage, string> {
                { Subscription.SubscriptionLanguage.English, "en" }
        };

        //--- Class Methods ---
        public static TwitterPublicConnection GetInstance() {
            return _instance ?? (_instance = new TwitterPublicConnection());
        }

        private static XUri GetNewSubscriptionQuery(Subscription subscription) {
             string lang;
            _subscriptionLanguagesMap.TryGetValue(subscription.Language, out lang);
            var uri = new XUri(SEARCH_URI).With("q", string.Join(",", subscription.Constraints.ToArray()));
            if(!string.IsNullOrEmpty(lang)) {
                uri = uri.With("lang", lang);
            }
            return uri;
        }

        //--- Constructors ---
        private TwitterPublicConnection() {}

        //--- Methods ---
        public IEnumerable<IMessage> GetMessages(IGuidGenerator guidGenerator, IDateTime dateTime, Subscription subscription, TimeSpan expiration) {
            var messages = new List<IMessage>();
            if(!subscription.Active) {
                return messages;
            }
            DreamMessage msg = Plug.New(subscription.GetSourceUri(Source.Twitter)).Get();
            if(!msg.IsSuccessful) {
                return messages;
            }
            var response = new JDoc(msg.ToText()).ToDocument("response");
            var results = response["results"];
            var ids = new List<ulong>();
            foreach(var result in results) {
                var id = ulong.MinValue;
                ulong.TryParse(result["id_str"].AsText, out id);
                if(id != ulong.MinValue) {
                    ids.Add(id);
                }
            }
           
            // build new request for next set of results
            var query = GetNewSubscriptionQuery(subscription);
        
            // fetch earlier results if they exist
            if(ids.Count > 0 && response["next_page"] != null) {
                ids.Sort();
                var maxId = ids.First() - 1;
                query = query.With("max_id", maxId.ToString()); 

                // adjust cursor to highest id processed
                var cursor = ulong.MinValue;
                ulong.TryParse(subscription.ResultsCursor, out cursor);
                if(ids.Last() > cursor) {
                    subscription.ResultsCursor = ids.Last().ToString();    
                }
            }
            else if(!string.IsNullOrEmpty(subscription.ResultsCursor)) {
                query = query.With("since_id", subscription.ResultsCursor); 
            }
            subscription.SetSourceUri(Source.Twitter, query);
            messages.AddRange(results.Select(result => GetMessage(guidGenerator, dateTime, result, expiration)));
            return messages;
        }

        public void SetNewSubscriptionQuery(Subscription subscription) {
            subscription.SetSourceUri(Source.Twitter, GetNewSubscriptionQuery(subscription));
        }
       
        public Identity GetIdentity(string name) {
            return GetIdentityByScreenName(name);
        }
    }
}
