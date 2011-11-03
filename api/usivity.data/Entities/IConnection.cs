using MindTouch.Dream;

namespace Usivity.Data.Entities {

    public interface IConnection {

        //--- Properties ---
        SourceIdentity Identity { get; set; }

        //--- Methods ---
        DreamHeaders GetHeaders();
    }
}
