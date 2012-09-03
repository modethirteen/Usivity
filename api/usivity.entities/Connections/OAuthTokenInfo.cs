using MindTouch.OAuth;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities.Connections {

    public class OAuthTokenInfo {

        //--- Fields ---
        private readonly string _id;

        //--- Properties ---
        public Source Source { get; private set; }
        public string OrganizationId { get; private set; }
        public string UserId { get; private set; }
        public OAuthRequestToken Token { get; private set;}

        //--- Constructors ---
        public OAuthTokenInfo(OAuthRequestToken token, Source source, IGuidGenerator guidGenerator, IOrganization organization, IUser user) {
            _id = guidGenerator.GenerateNewObjectId();
            Source = source;
            Token = token;
            OrganizationId = organization.Id;
            UserId = user.Id;
        }
    }
}
