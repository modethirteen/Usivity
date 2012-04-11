using System;

namespace Usivity.Data.Connections {

    public interface IOAuthRequest {

        //--- Properties ---
        Uri AuthorizeUri { get; }
        string Token { get; }
        string TokenSecret { get; }
        string Verifier { get; set; }
        DateTime Created { get; }
    }
}
