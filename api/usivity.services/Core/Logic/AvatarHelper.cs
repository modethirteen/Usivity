using System;
using MindTouch.Dream;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Services.Core.Logic {

    public class AvatarHelper : IAvatarHelper {

        //--- Fields ---
        private readonly ICurrentContext _context;

        //--- Constructors ---
        public AvatarHelper(ICurrentContext context) {
            _context = context;            
        }

        public XUri GetGravatarUri(string email) {
            var hash = StringUtil.ComputeHashString(email.ToLowerInvariant().Trim());
            var uri = (_context.ApiUri.IsSecure()) ? new XUri("https://secure.gravatar.com/avatar") : new XUri("http://www.gravatar.com/avatar");
            if(_context.DefaultAvatarUri != null) {
                uri = uri.With("d", _context.DefaultAvatarUri.ToString());
            }
            return uri.At(hash);
        }

        public XUri GetAvatarUri(Identity identity) {
            XUri uri;
            var secure = _context.ApiUri.IsSecure();
            if(secure && identity.AvatarSecure != null) {
                uri = identity.AvatarSecure;
            }
            else {
                uri = identity.Avatar; 
            }
            return uri;
        }
    }
}
