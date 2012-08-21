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
            Modified = dateTime.UtcNow;
            Source = Source.Twitter;
        }

        //--- Properties ---
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public Source Source { get; private set; }
        public Identity Identity { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public OAuthRequestToken OAuthRequest { get; set;}
        public OAuthAccessToken OAuthAccess { get; set; }
        public bool Active { get { return Identity != null && OAuthAccess != null; } }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "connection";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            var doc = new XDoc(resource)
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .Elem("type", "oauth");
            if(Active) {
                doc.Start("identity")
                    .Attr("id", Identity.Id)
                    .Elem("name", Identity.Name)
                    .Elem("uri.avatar", Identity.Avatar)
                .End()
                .Elem("active", true);
            }
            else {
                doc.Elem("uri.authorize", OAuthRequest.AuthorizeUri.ToString())
                    .Elem("active", false);
            }
            return doc; 
        }
    }
}
