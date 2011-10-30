using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Core.Libraries.Json;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Sources;
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [DreamFeature("GET:sources", "Get all source networks")]
        public Yield GetSources(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
        public Yield GetSource(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
        public Yield GetSourceConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
        public Yield UpdateSourceConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
        [DreamFeatureParam("constraints", "string", "Comma seperated list of constraints")]
        public Yield PostSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Id == context.GetParam<string>("sourceid"));
            if(source == null) {
                response.Return(DreamMessage.NotFound("Network source does not exist"));
                yield break;
            }
            var connection = UsivityContext.Current.User.GetConnection(source.Id);
            if(connection == null) {
                throw new DreamBadRequestException(string.Format("Cannot subscribe to unauthenticated source network, \"{0}\"", source.Id));
            }
            
            var constraints = context.GetParam<string>("constraints").Split(',');
            var subscription = source.GetNewSubscription(constraints);
            source.Subscriptions.Add(subscription);
            _data.SaveSubscription(subscription);

            var doc = GetSubscriptionXml(subscription);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{sourceid}/subscriptions", "Get all subscriptions to a source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        public Yield GetSubscriptions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
        public Yield GetSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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

        [DreamFeature("DELETE:sources/{source}/subscriptions/{subscriptionid}", "Delete subscription to a source network")]
        [DreamFeatureParam("sourceid", "string", "Source network id")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        public Yield DeleteSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
