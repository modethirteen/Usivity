using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

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
