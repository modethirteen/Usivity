using MindTouch.Xml;
using Usivity.Entities;

namespace Usivity.Services.Core.Logic {

    public interface ISubscriptions {

        //--- Methods ---
        Subscription GetSubscription(string id);
        Subscription GetNewSubscription(XDoc info);
        XDoc GetSubscriptionXml(Subscription subscription, string relation = null);
        XDoc GetSubscriptionsXml();
        void SaveSubscription(Subscription subscription);
        void DeleteSubscription(Subscription subscription); 
    }
}
