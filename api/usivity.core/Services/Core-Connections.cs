using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Connections;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        [DreamFeature("GET:connections", "Get all source connections")]
        internal Yield GetConnections(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = UsivityContext.Current.Organization;
            var doc = new XDoc("connections")
                .Attr("count", organization.Connections.Count())
                .Attr("href", _connectionsUri);
            foreach(var connection in organization.Connections) {
                doc.Add(GetConnectionXml(connection));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:connections/{connectionid}", "Get source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield GetConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var connection = UsivityContext.Current.Organization
                .GetConnection(context.GetParam<string>("connectionid"));
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            var doc = GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("POST:connections", "Add new source connection")]
        [DreamFeatureParam("source", "{twitter}", "Source connection type")]
        internal Yield PostConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = UsivityContext.Current.Organization;
            IConnection connection;
            switch(context.GetParam<string>("source").ToLowerInvariant()) {
                case "twitter":
                    connection = _twitterConnectionFactory.NewTwitterConnection();
                    organization.SetConnection(connection);
                    break;
                default:
                    throw new DreamBadRequestException("Invalid source connection type");
            }
            _data.Organizations.Save(organization);
            var doc = GetConnectionXml(connection);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("PUT:connections/{connectionid}", "Update your source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield UpdateConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = UsivityContext.Current.Organization;
            var id = context.GetParam<string>("connectionid");
            var connection = organization.GetConnection(id);
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            connection.Update(GetRequestXml(request));
            organization.SetConnection(connection);
            _data.Organizations.Save(organization);
            response.Return(DreamMessage.Ok(MimeType.TEXT, "Source connection settings have been saved"));
            yield break;
        }

        [DreamFeature("DELETE:connections/{connectionid}", "Delete source connection")]
        [DreamFeatureParam("connectionid", "string", "Source connection id")]
        internal Yield DeleteConnection(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = UsivityContext.Current.Organization;
            var id = context.GetParam<string>("connectionid");
            var connection = organization.GetConnection(id);
            if(connection == null) {
                response.Return(DreamMessage.NotFound("Source connection does not exist"));
                yield break;
            }
            organization.RemoveConnection(id);
            _data.Organizations.Save(organization);
            response.Return(DreamMessage.Ok());
            yield break;
        }

        //--- Methods ---
        private XDoc GetConnectionXml(IConnection connection) {
            return connection.ToDocument().Attr("href", _connectionsUri.At(connection.Id));
        }
    }
}
