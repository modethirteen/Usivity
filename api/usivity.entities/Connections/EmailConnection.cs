using System;
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
        public DateTime Created { get; private set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseCramMd5 { get; set; }
        public bool UseSsl { get; set; }
    }
}
