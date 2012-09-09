using System;
using MindTouch.Dream;

namespace Usivity.Services.Clients {

    public class ConnectionResponseException : Exception {

        //--- Fields ---
        public readonly DreamStatus Status;

        //--- Constructors ---
        public ConnectionResponseException(DreamStatus status, string message, Exception e) : base(message, e) {
            Status = status;
        }

        public ConnectionResponseException(DreamStatus status, string message, XUri request, DreamMessage response, Exception e) :
            base(string.Format("\n{0}\n---\nRequest: {1}\n---\nResponse: {2}\n", message, request, response.ToDocument().ToCompactString()), e) {
                Status = status;
        }
    }
}
