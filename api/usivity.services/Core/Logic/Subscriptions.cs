﻿using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Util;

namespace Usivity.Services.Core.Logic {

    public class Subscriptions : ISubscriptions {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganizations _organizations;
        private readonly IGuidGenerator _guidGenerator;

        //--- Constructors ---
        public Subscriptions(IGuidGenerator guidGenerator, IUsivityDataCatalog data, ICurrentContext context, IOrganizations organizations) {
            _context = context;
            _organizations = organizations;
            _data = data;
            _guidGenerator = guidGenerator;
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
            return new Subscription(_guidGenerator, _organizations.CurrentOrganization, constraints, lang);
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
            var resource = "subscription";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", subscription.Id)
                .Attr("href", _context.ApiUri.At("subscriptions", subscription.Id))
                .Elem("active", subscription.Active ? "true" : "false")
                .Elem("constraints", string.Join(",", subscription.Constraints.ToArray()))
                .EndAll();   
        }
    }
}
