namespace Usivity.Data.Connections {

    public interface IOAuthAccess {
        
        //--- Properties ---
        string Token { get; }
        string TokenSecret { get; }
    }
}