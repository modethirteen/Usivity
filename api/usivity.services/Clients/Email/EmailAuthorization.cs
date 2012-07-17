using System;
using MindTouch.Xml;

namespace Usivity.Services.Clients.Email {
    public class EmailAuthorization {

        //--- Properties ---
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public bool UseCramMd5 { get; set; }

        //--- Constructors ---
        public EmailAuthorization(XDoc config) {
            Host = config["host"].AsText;
            Username = config["username"].AsText;
            Password = config["password"].AsText;
            Port = config["port"].AsInt ?? 143;
            UseSsl = config["ssl"].AsBool ?? false;
            var auth = config["auth.method"].AsText;
            if(!string.IsNullOrEmpty(auth)) {
                auth = auth.ToLowerInvariant();
            }
            switch(auth) {
                case "crammd5":
                    UseCramMd5 = true;
                    break;
                case "login":
                case null:
                    UseCramMd5 = false;
                    break;
                default:
                    throw new Exception("\"auth.method\" element is invalid. Allowed values: login, crammd5");
            }
        }
    }
}
