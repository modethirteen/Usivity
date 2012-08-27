namespace Usivity.Services.Clients.Email {

    public class EmailClientConfig {
    
        //--- Properties ---
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
