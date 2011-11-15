﻿using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Sources;
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [DreamFeature("GET:sources", "Get all source networks")]
        protected Yield GetSources(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = new XDoc("sources")
                .Attr("count", _sources.Count())
                .Attr("href", _sourcesUri);
            foreach(var source in _sources) {
                doc.Add(GetSourceXml(source, UsivityContext.Current.User));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{sourceid}", "Get source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        protected Yield GetSource(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Source network does not exist"));
                yield break;
            }
            var doc = GetSourceXml(source, UsivityContext.Current.User);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{sourceid}/connection", "Get your source network connection")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        protected Yield GetSourceConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Source network does not exist"));
                yield break;
            }
            var doc = GetSourceConnectionXml(source, UsivityContext.Current.User);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("PUT:sources/{sourceid}/connection", "Update your source network connection")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        protected Yield UpdateSourceConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Source network does not exist"));
                yield break;
            }
            var user = UsivityContext.Current.User;

            //TODO: parse document instead of params
            var connectionRequest = new XDoc("connection")
                .Start("oauth")
                .Elem("token", context.GetParam<string>("oauth_token"))
                .Elem("verifier", context.GetParam<string>("oauth_verifier"))
                .EndAll();
            connectionRequest.Elem("userid", user.Id);
            var connection = source.GetNewConnection(connectionRequest);
            user.SetConnection(source.Id, connection);
            _data.SaveUser(user);

            response.Return(DreamMessage.Ok(MimeType.TEXT, "Connection settings have been saved"));
            yield break;
        }

        [DreamFeature("POST:sources/{sourceid}/subscriptions", "Subscribe to a source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        protected Yield PostSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Network source does not exist"));
                yield break;
            }
            var connection = UsivityContext.Current.User.GetConnection(source.Id);
            if(connection == null) {
                throw new DreamBadRequestException(string.Format("Cannot subscribe to unconnected source network, \"{0}\"", source.Id));
            }
            
            var subscriptionDoc = GetRequestXml(request);

            var lang = Subscription.SubscriptionLanguage.English;
            var langKey = subscriptionDoc["language"].AsText ?? string.Empty;
            if(!string.IsNullOrEmpty(langKey) && !Subscription.SubscriptionLanguages.TryGetValue(langKey, out lang)) {
                response.Return(DreamMessage.BadRequest("Requested language key \"" + langKey + "\" is unsupported"));
                yield break;
            }
            var constraints = subscriptionDoc["constraints/constraint"].Select(constraint => constraint.AsText).ToList();

            var subscription = source.GetNewSubscription(constraints, lang);
            subscription.UserId = UsivityContext.Current.User.Id;
            source.Subscriptions.Add(subscription);
            _data.SaveSubscription(subscription);

            var doc = GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{sourceid}/subscriptions", "Get all subscriptions to a source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        protected Yield GetSubscriptions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Source network does not exist"));
                yield break;
            }
            var doc = GetSubscriptionsXml(source, UsivityContext.Current.User);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{sourceid}/subscriptions/{subscriptionid}", "Get subscription to a source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        protected Yield GetSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Source network does not exist"));
                yield break;
            }
            var subscription = _data.GetSubscription(context.GetParam<string>("subscriptionid"));
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            var doc = GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:sources/{sourceid}/subscriptions/{subscriptionid}", "Delete subscription to a source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        protected Yield DeleteSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Source network does not exist"));
                yield break;
            }
            var subscription = _data.GetSubscription(context.GetParam<string>("subscriptionid"));
            if(subscription == null) {
                response.Return(DreamMessage.NotFound("Subscription does not exist"));
                yield break;
            }
            source.Subscriptions.Remove(subscription);
            _data.DeleteSubscription(subscription);
            response.Return(DreamMessage.Ok());
            yield break;
        }

        //--- Methods ---
        private XDoc GetSourceXml(ISource source, User user) {
            var connectionDoc = GetSourceConnectionXml(source, user);
            var doc = new XDoc("source")
                .Attr("id", source.Id)
                .Attr("href", _sourcesUri.At(source.Id))
                .Add(connectionDoc);
            doc.Add(GetSubscriptionsXml(source, user)).EndAll();
            return doc;
        }

        private XDoc GetSourceConnectionXml(ISource source, User user) {
            return source.GetConnectionXml(user)
                .Attr("href", _sourcesUri.At(source.Id, "connection"));
        }

        private XDoc GetSubscriptionsXml(ISource source, User user) {
            var subscriptions = _data.GetSubscriptions(source.Id, user);
            var doc = new XDoc("subscriptions")
                .Attr("count", subscriptions.Count())
                .Attr("href", _sourcesUri.At(source.Id, "subscriptions"));
            foreach(var subscription in subscriptions) {
                doc.Add(GetSubscriptionXml(subscription));
            }
            doc.EndAll();
            return doc;
        }

        private XDoc GetSubscriptionXml(Subscription subscription, string relation = null) {
            return subscription.ToDocument(relation)
                .Attr("href", _sourcesUri.At(subscription.Source, "subscriptions", subscription.Id));
        }
    }
}
