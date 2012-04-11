using System;
using System.Collections.Generic;
using AE.Net.Mail;
using Usivity.Data.Entities;

namespace Usivity.Data.Connections {

    class ImapConnection : IConnection {

        //--- Properties ---
        public IIdentity Identity { get; private set; }
        public ImapClient Client { get; private set; }

        //--- Constructors ---
        /*public ImapConnection(ImapConnectionConfig config) {
            Client = new ImapClient(
                config.Host,
                config.Username,
                config.Password,
                ImapClient.AuthMethods.Login,
                config.Port,
                config.UseSsl,
                true
                );
        }*/

        //--- Methods ---
        public IList<Message> GetMessages(Subscription subscription) {
            throw new NotImplementedException();
        }

        public Message PostReplyMessage(Message message, string reply) {
            throw new NotImplementedException();
        }
    }
}
