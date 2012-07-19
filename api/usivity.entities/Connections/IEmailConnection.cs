using System;

namespace Usivity.Entities.Connections {

    public interface IEmailConnection : IConnection, ICloneable {

        //--- Properties ---
        string Host { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        int Port { get; set; }
        bool UseCramMd5 { get; set; }
        bool UseSsl { get; set; }
    }
}
