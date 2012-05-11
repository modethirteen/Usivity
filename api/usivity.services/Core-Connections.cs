using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Connections;
using Usivity.Entities;

namespace Usivity.Services {
    using Core.Logic;
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("GET:connections", "Get all source connections")]
        internal Yield GetConnections(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IConnections>(context).GetConnectionsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
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
        
        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("POST:connections", "Add new source connection")]
        [DreamFeatureParam("source", "{twitter,email}", "Source connection type")]
        internal Yield PostConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            IConnection connection;
            var organizations = Resolve<IOrganizations>(context);
            switch(context.GetParam<string>("source").ToLowerInvariant()) {
                case "twitter":
                    connection = _twitterConnectionFactory.NewConnection(organizations.CurrentOrganization);
                    break;
                case "email":
                    connection = _emailConnectionFactory.NewConnection(organizations.CurrentOrganization);
                    break;
                default:
                    response.Return(DreamMessage.BadRequest("Invalid source connection type"));
                    yield break;
            }
            var connections = Resolve<IConnections>(context);
            connections.SaveConnection(connection);
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
        [DreamFeature("PUT:connections/{connectionid}", "Update your source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield UpdateConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var connections = Resolve<IConnections>(context);
            var connection = connections.GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            try {
                connection.Update(GetRequestXml(request));
            }
            catch(DreamAbortException e) {
                response.Return(e.Response);
                yield break;
            }
            connections.SaveConnection(connection);
            var doc = connections.GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Admin)]
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
