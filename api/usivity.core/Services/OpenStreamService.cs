using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Data;

namespace Usivity.Core.Services {
    using OpenStream;
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity Open Stream Service", "",
        SID = new[] {
            "sid://usivity.com/2011/01/openstream"
        }
    )]
    class OpenStreamService : DreamService {

        //--- Constants ---
        internal const uint MINUTES_TO_REFRESH = 15;

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private double _refresh;
        private IList<ISource> _sources;
        private MongoCollection _openstream;
        private MongoCollection _subscriptions;
        private MongoCollection _oauthTokens;
        private Plug _core;
        private string _apikey;

        //--- Features ---
        [DreamFeature("GET:messages", "Get open stream messages")]
        [DreamFeatureParam("count", "int?", "Max messages to receive ranging 1 - 100 (default: 10)")]
        public Yield GetMessages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var count = context.GetParam("count", 10);
            var query = Query.And(
                Query.LTE("NextAccess", DateTime.UtcNow),
                Query.EQ("Active", true)
                );
            var messages = _openstream.FindAs<Message>(query)
                .SetLimit(count)
                .SetSortOrder("NextAccess");
            var doc = new XDoc("messages");
            foreach(var message in messages) {
                doc.Add(message.ToDocument());
                _openstream.Update(
                    Query.EQ("_id", message.Id),
                    Update.Set("NextAccess", DateTime.UtcNow.AddMinutes(_refresh))
                    );
            }
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:messages/{messageid}", "Get open stream message")]
        [DreamFeatureParam("messageid", "string", "Open stream message id")]
        public Yield GetMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var message = GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var doc = message.ToDocument();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:messages/{messageid}", "Delete open stream message")]
        [DreamFeatureParam("messageid", "string", "Open stream message id")]
        public Yield DeleteMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var message = GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            _openstream.Remove(Query.EQ("_id", message.Id));
            response.Return(DreamMessage.Ok());
            yield break;
        }
       
        [DreamFeature("POST:messages/{messageid}", "Post message in reply")]
        [DreamFeatureParam("message", "string", "Message id to reply to")]
        public Yield PostMessageReply(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();    
        }

        [DreamFeature("GET:sources", "Get all available source networks")]
        public Yield GetSources(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = new XDoc("sources");

            // TODO: use user id and refactor for single db request
            foreach(var source in _sources) {
                var auth = _oauthTokens
                    .FindOneAs<OAuthToken>(Query.EQ("Source", source.Name));
                doc.Start("source")
                    .Attr("name", source.Name)
                    .Elem("authentication", auth != null ? "enabled" : "disabled")
                    .End();
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{source}", "Get source network by name")]
        [DreamFeatureParam("source", "string", "Source network name")]
        public Yield GetSource(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }

            // TODO: use user id and refactor for single db request
            var auth = _oauthTokens
                    .FindOneAs<OAuthToken>(Query.EQ("Source", source.Name));
            var doc = new XDoc("source")
                    .Attr("name", source.Name)
                    .Elem("authentication", auth != null ? "enabled" : "disabled")
                    .EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("PUT:sources/{source}/authentication", "Update your authentication token for a source network by name")]
        [DreamFeatureParam("source", "string", "Source network name")]
        public Yield AuthenticateSource(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }
            var auth = _oauthTokens.FindOneAs<OAuthToken>(Query.EQ("Source", source.Name))
                ?? new OAuthToken { Source = source.Name };
            auth.Value = request.ToText();
            _oauthTokens.Save(auth);
            foreach(var subscription in source.Subscriptions) {
                subscription.Active = true;
                _subscriptions.Save(subscription);
            }
            response.Return(DreamMessage.Ok(MimeType.TEXT, "Authentication settings have been saved"));
            yield break;
        }

        [DreamFeature("DELETE:sources/{source}/authentication", "Delete authentication token for a source network by name")]
        [DreamFeatureParam("source", "string", "Source network name")]
        public Yield DeleteSourceAuthentication(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }
            _oauthTokens.Remove(Query.EQ("Source", source.Name));
            foreach(var subscription in source.Subscriptions) {
                subscription.Active = false;
                _subscriptions.Save(subscription);
            }
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("POST:sources/{source}/subscriptions", "Add a subscription to a network source")]
        [DreamFeatureParam("source", "string", "Network source")]
        [DreamFeatureParam("constraints", "string", "Comma seperated list of constraints")]
        public Yield PostSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }
            var auth = _oauthTokens.FindOneAs<OAuthToken>(Query.EQ("Source", source.Name));
            if(auth == null) {
                throw new DreamBadRequestException(string.Format("Cannot subscribe to unauthenticated source \"{0}\"", source.Name));
            }
            var constraints = context.GetParam<string>("constraints").Split(',');
            var subscription = new Subscription(source.Name) {
                Uri = source.GenerateUriWithConstraints(constraints),
                Constraints = constraints
            };

            source.Subscriptions.Add(subscription);
            _subscriptions.Insert(subscription);
            
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("GET:sources/{source}/subscriptions", "Get all subscriptions to a network source")]
        [DreamFeatureParam("source", "string", "Network source")]
        public Yield GetSubscriptions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }
            var subscriptions = _subscriptions.FindAs<Subscription>(Query.EQ("Source", source.Name));
            var doc = new XDoc("subscriptions").Attr("source", source.Name);
            foreach(var subscription in subscriptions) {
                doc.Start("subscription")
                    .Attr("id", subscription.Id)
                    .Attr("active", subscription.Active)
                    .Value(string.Join(",", subscription.Constraints.ToArray()))
                    .End();
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:sources/{source}/subscriptions/{subscriptionid}", "Get subscription to a network source")]
        [DreamFeatureParam("source", "string", "Network source")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        public Yield GetSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }
            var id = context.GetParam<string>("subscriptionid");
            var subscription = _subscriptions.FindOneByIdAs<Subscription>(id);
            if(subscription == null) {
                throw new DreamNotFoundException("Subscription does not exist");
            }
            var doc = new XDoc("subscription")
                .Attr("id", subscription.Id)
                .Attr("active", subscription.Active)
                .Value(string.Join(",", subscription.Constraints.ToArray()))
                .EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:sources/{source}/subscriptions/{subscriptionid}", "Delete subscription to a network source")]
        [DreamFeatureParam("source", "string", "Network source")]
        [DreamFeatureParam("subscriptionid", "string", "Subscription id")]
        public Yield DeleteSubscription(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var source = _sources.FirstOrDefault(s => s.Name == context.GetParam<string>("source"));
            if(source == null) {
                throw new DreamNotFoundException("Network source does not exist");
            }
            var id = context.GetParam<string>("subscriptionid");
            var subscription = _subscriptions.FindOneByIdAs<Subscription>(id);
            if(subscription == null) {
                throw new DreamNotFoundException("Subscription does not exist");
            }
            source.Subscriptions.Remove(subscription);
            _subscriptions.Remove(Query.EQ("_id", subscription.Id));
            response.Return(DreamMessage.Ok());
            yield break;
        }

        //--- Methods ---
        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());
            _core = Plug.New(config["core.uri"].AsText);
            _apikey = config["apikey"].AsText ?? string.Empty;

            var db = UsivityDataSession.CurrentSession;
            _openstream = db.GetCollection<Message>("messages");
            var indexes = new IndexKeysBuilder().Ascending("Source", "SourceId");
            _openstream.EnsureIndex(indexes, IndexOptions.SetUnique(true));
            _subscriptions = db.GetCollection<Subscription>("subscriptions");
            _oauthTokens = db.GetCollection<OAuthToken>("oauth_tokens");

            // setup sources
            _sources = new List<ISource> {
                new TwitterSource()
            };
            foreach(var source in _sources) {
                var subscriptions = _subscriptions
                    .FindAs<Subscription>(Query.EQ("Source", source.Name));
                foreach(var subscription in subscriptions) {
                    source.Subscriptions.Add(subscription);
                }               
            }

            _refresh = config["refresh"].AsDouble ?? MINUTES_TO_REFRESH;
            TaskTimerFactory.Current.New(TimeSpan.Zero, QueueMessages, null, TaskEnv.None);
            result.Return();
        }

        private void QueueMessages(TaskTimer tt) {

            // find expired messages and remove
            _openstream.FindAndRemove(Query.LTE("Expires", DateTime.UtcNow), SortBy.Null);

            // queue sources and update persisted query uris
            var messages = new List<Message>();
            foreach(var source in _sources) {
                messages.AddRange(source.GetMessages());
                foreach(var subscription in source.Subscriptions) {
                    _subscriptions.Save(subscription);
                }
            }

            var span = DateTime.UtcNow.Subtract(TimeSpan.FromDays(4));
            foreach (var message in messages.Where(message => message.Timestamp >= span)) {
                _openstream.Insert(message);
            }
            tt.Change(TimeSpan.FromMinutes(1), TaskEnv.None);
        }

        private Message GetMessage(string id) {
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("Active", true)
                );
            return _openstream.FindOneAs<Message>(query);
        }
    }
}
