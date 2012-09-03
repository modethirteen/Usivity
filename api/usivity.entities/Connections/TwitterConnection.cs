using System;
using MindTouch.OAuth;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities.Connections {

    public class TwitterConnection : ITwitterConnection {

        //--- Constructors ---
        public TwitterConnection(IGuidGenerator guidGenerator, IOrganization organization, IDateTime dateTime) {
            Id = guidGenerator.GenerateNewObjectId();
            OrganizationId = organization.Id;
            Created = dateTime.UtcNow;
            Source = Source.Twitter;
        }

        //--- Properties ---
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public Source Source { get; private set; }
        public Identity Identity { get; set; }
        public DateTime Created { get; set; }
        public OAuthAccessToken OAuthAccess { get; set; }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "connection";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .Start("identity")
                    .Attr("id", Identity.Id)
                    .Elem("name", Identity.Name)
                    .Elem("uri.avatar", Identity.Avatar)
                .End()
                .Elem("created", Created.ToISO8601String());
        }
    }
}
