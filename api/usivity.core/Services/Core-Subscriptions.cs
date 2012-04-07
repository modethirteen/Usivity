using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Connections;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [DreamFeature("POST:subscriptions", "Add a new subscription")]
        internal Yield PostSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscriptionDoc = GetRequestXml(request);
            var lang = Subscription.SubscriptionLanguage.English;
            var langKey = subscriptionDoc["language"].AsText ?? string.Empty;
            if(!string.IsNullOrEmpty(langKey) && !Subscription.SubscriptionLanguages.TryGetValue(langKey, out lang)) {
                response.Return(DreamMessage.BadRequest("Requested language key \"" + langKey + "\" is unsupported"));
                yield break;
            }
            var organization = UsivityContext.Current.Organization;
            var constraints = subscriptionDoc["constraints/constraint"].Select(constraint => constraint.AsText).ToList();
            var subscription = new Subscription(organization, constraints, lang);
            _data.Subscriptions.Save(subscription);
            var doc = GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:subscriptions", "Get all subscriptions")]
        internal Yield GetSubscriptions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscriptions = _data.Subscriptions.Get(UsivityContext.Current.Organization);
            var doc = new XDoc("subscriptions")
                .Attr("count", subscriptions.Count())
                .Attr("href", _subscriptionsUri);
            foreach(var subscription in subscriptions) {
                doc.Add(GetSubscriptionXml(subscription));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:subscriptions/{subscriptionid}", "Get a subscription")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        internal Yield GetSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscription = _data.Subscriptions
                .Get(context.GetParam<string>("subscriptionid"), UsivityContext.Current.Organization);
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            var doc = GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:subscriptions/{subscriptionid}", "Delete a subscription")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        internal Yield DeleteSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscription = _data.Subscriptions
                .Get(context.GetParam<string>("subscriptionid"), UsivityContext.Current.Organization);
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            _data.Subscriptions.Delete(subscription);
            response.Return(DreamMessage.Ok());
            yield break;
        }

        //--- Methods ---
        private XDoc GetSubscriptionXml(Subscription subscription, string relation = null) {
            return subscription.ToDocument(relation).Attr("href", _subscriptionsUri.At(subscription.Id));
        }
    }
}
