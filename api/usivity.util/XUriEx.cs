using System;
using MindTouch.Dream;

namespace Usivity.Util {

    public static class XUriEx {

        public static bool IsSecure(this XUri uri) {
            return uri.Scheme.StartsWithInvariantIgnoreCase("https");    
        }
    }
}
