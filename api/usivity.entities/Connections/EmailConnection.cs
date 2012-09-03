using System;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities.Connections {

    public class EmailConnection : IEmailConnection {

        //--- Constructors ---
        public EmailConnection(IGuidGenerator guidGenerator, IOrganization organization, IDateTime dateTime) {
            Id = guidGenerator.GenerateNewObjectId();
            OrganizationId = organization.Id;
            Created = dateTime.UtcNow;
            Source = Source.Email;
        }

        //--- Properties ---
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public Source Source { get; private set; }
        public Identity Identity { get; set; }
        public DateTime Created { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseCramMd5 { get; set; }
        public bool UseSsl { get; set; }

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
