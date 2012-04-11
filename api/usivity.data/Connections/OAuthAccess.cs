namespace Usivity.Data.Connections {

    public class OAuthAccess : IOAuthAccess {

        //--- Properties ---
        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }
}
