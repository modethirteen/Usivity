using System;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities.Connections {

    public class EmailConnection : IEmailConnection {

        //--- Constructors ---
        public EmailConnection(IGuidGenerator guidGenerator, IOrganization organization) {
            Id = guidGenerator.GenerateNewObjectId();
            LastSearch = DateTime.MinValue;
            OrganizationId = organization.Id;
            Source = Source.Email;
        }

        //--- Properties ---
        public string Id { get; private set; }
        public string OrganizationId { get; private set; }
        public Source Source { get; private set; }
        public Identity Identity { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseCramMd5 { get; set; }
        public bool UseSsl { get; set; }
        public bool Active { get { return IsActive(); } }
        public DateTime LastSearch { get; set; }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "connection";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            var doc = new XDoc(resource)
                .Attr("id", Id)
                .Elem("source", Source.ToString().ToLowerInvariant())
                .Elem("type", "imap");
            if(Active) {
                doc.Start("identity")
                    .Attr("id", Identity.Id)
                    .Elem("name", Identity.Name)
                .End()
                .Elem("active", true);
            }
            else {
                doc.Elem("active", false);
            }
            return doc; 
        }

        private bool IsActive() {
            return Identity != null && Host != null && Username != null;
        }

        public object Clone() {
            return MemberwiseClone();
        }
    }
}
