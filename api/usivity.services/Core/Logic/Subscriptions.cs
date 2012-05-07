using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;

namespace Usivity.Services.Core.Logic {

    public class Subscriptions : ISubscriptions {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganizations _organizations;

        //--- Constructors ---
        public Subscriptions(IUsivityDataCatalog data, ICurrentContext context, IOrganizations organizations) {
            _context = context;
            _organizations = organizations;
            _data = data;
        }

        //--- Methods ---
        public Subscription GetSubscription(string id) {
            return _data.Subscriptions.Get(id, _organizations.CurrentOrganization);
        }

        public Subscription GetNewSubscription(XDoc info) {
            var lang = Subscription.SubscriptionLanguage.English;
            var langKey = info["language"].AsText ?? string.Empty;
            if(!string.IsNullOrEmpty(langKey) && !Subscription.SubscriptionLanguages.TryGetValue(langKey, out lang)) {
                var response = DreamMessage
                    .BadRequest(string.Format("Requested language key \"{0}\" is unsupported", langKey));
                throw new DreamAbortException(response);
            }
            var constraints = info["constraints/constraint"].Select(constraint => constraint.AsText).ToList();
            return new Subscription(_organizations.CurrentOrganization, constraints, lang);
        }

        public void SaveSubscription(Subscription subscription) {
            _data.Subscriptions.Save(subscription);
        }

        public void DeleteSubscription(Subscription subscription) {
            _data.Subscriptions.Delete(subscription);
        }

        public XDoc GetSubscriptionsXml() {
            var subscriptions = _data.Subscriptions.Get(_organizations.CurrentOrganization);
            var doc = new XDoc("subscriptions")
                .Attr("count", subscriptions.Count())
                .Attr("href", _context.ApiUri.At("subscriptions"));
            foreach(var subscription in subscriptions) {
                doc.Add(GetSubscriptionXml(subscription));
            }
            doc.EndAll();
            return doc;
        }

        public XDoc GetSubscriptionXml(Subscription subscription, string relation = null) {
            return subscription.ToDocument(relation)
                .Attr("href", _context.ApiUri.At("subscriptions", subscription.Id));
        }
    }
}
