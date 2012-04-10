using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Core.Services.Logic;
using Usivity.Data.Connections;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [DreamFeature("GET:connections", "Get all source connections")]
        internal Yield GetConnections(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IConnections>(context).GetConnectionsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:connections/{connectionid}", "Get source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield GetConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var connections = Resolve<IConnections>(context);
            var connection = connections.GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("POST:connections", "Add new source connection")]
        [DreamFeatureParam("source", "{twitter}", "Source connection type")]
        internal Yield PostConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            IConnection connection;
            switch(context.GetParam<string>("source").ToLowerInvariant()) {
                case "twitter":
                    connection = _twitterConnectionFactory.NewTwitterConnection();
                    break;
                default:
                    throw new DreamBadRequestException("Invalid source connection type");
            }
            var connections = Resolve<IConnections>(context);
            connections.SaveConnection(connection);
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("PUT:connections/{connectionid}", "Update your source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield UpdateConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var connections = Resolve<IConnections>(context);
            var connection = connections.GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            connection.Update(GetRequestXml(request));
            connections.SaveConnection(connection);
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:connections/{connectionid}", "Delete source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield DeleteConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var connections = Resolve<IConnections>(context);
            var connection = connections.GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            connections.DeleteConnection(connection);
            response.Return(DreamMessage.Ok());
            yield break;
        }
    }
}
