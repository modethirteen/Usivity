namespace Usivity.Connections {
    
    public interface IOAuthAccess {
        
        //--- Properties ---
        string Token { get; }
        string TokenSecret { get; }
    }

    public class OAuthAccess : IOAuthAccess {

        //--- Properties ---
        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }
}
