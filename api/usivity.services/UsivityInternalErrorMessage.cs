using MindTouch.Dream;

namespace Usivity.Services {

    public class UsivityInternalErrorMessage : DreamMessage {

        public UsivityInternalErrorMessage() :
            base(DreamStatus.InternalError, null, MimeType.TEXT_UTF8, "Something went wrong, please try your request again.") {}
    }
}
