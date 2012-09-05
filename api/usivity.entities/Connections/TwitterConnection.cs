using System;
using MindTouch.OAuth;
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
        public DateTime Created { get; private set; }
        public OAuthAccessToken OAuthAccess { get; set; }
    }
}
