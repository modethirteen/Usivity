using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Core.Services.Logic;
using Usivity.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:subscriptions", "Get all subscriptions")]
        internal Yield GetSubscriptions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<ISubscriptions>(context).GetSubscriptionsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:subscriptions/{subscriptionid}", "Get a subscription")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        internal Yield GetSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscriptions = Resolve<ISubscriptions>(context);
            var subscription = subscriptions.GetSubscription(context.GetParam<string>("subscriptionid"));
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            var doc = subscriptions.GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("POST:subscriptions", "Add a new subscription")]
        internal Yield PostSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscriptions = Resolve<ISubscriptions>(context);
            DreamMessage msg;
            try {
                var subscription = subscriptions.GetNewSubscription(GetRequestXml(request));
                subscriptions.SaveSubscription(subscription);
                msg = DreamMessage.Ok(subscriptions.GetSubscriptionXml(subscription));
            }
            catch(DreamAbortException e) {
                msg = e.Response; 
            }
            response.Return(msg);
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("DELETE:subscriptions/{subscriptionid}", "Delete a subscription")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        internal Yield DeleteSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var subscriptions = Resolve<ISubscriptions>(context);
            var subscription = subscriptions.GetSubscription(context.GetParam<string>("subscriptionid"));
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            subscriptions.DeleteSubscription(subscription);
            response.Return(DreamMessage.Ok());
            yield break;
        }
    }
}
