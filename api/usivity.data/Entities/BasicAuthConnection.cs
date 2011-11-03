using System;
using System.Text;
using MindTouch.Dream;

namespace Usivity.Data.Entities {

    public class BasicAuthConnection : IConnection {

        //--- Properties ---
        public string HttpUsername { get; private set; }
        public string HttpPassword { get; private set; }
        public SourceIdentity Identity { get; set; }

        //--- Constructors ---
        public BasicAuthConnection(string username, string password) {
            HttpUsername = username;
            HttpPassword = password;
        }

        //--- Methods ---
        public DreamHeaders GetHeaders() {
            var auth = HttpUsername + ":" + HttpPassword;
            var headers = new DreamHeaders {
                Authorization = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(auth))
            };
            return headers;
        }
    }
}
