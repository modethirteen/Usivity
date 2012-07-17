using System;
using MindTouch.Dream;

namespace Usivity.Services.Clients {

    public class ConnectionResponseException : Exception {

        //--- Fields ---
        public readonly DreamStatus Status;

        //--- Constructors ---
        public ConnectionResponseException(DreamStatus status, string message, Exception exception) : base(message, exception) {
            Status = status;
        }
    }
}
