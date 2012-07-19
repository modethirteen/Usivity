using Usivity.Entities.Connections;
using Usivity.Util;

namespace Usivity.Services.Clients.Twitter {

    public class TwitterClientFactory : ITwitterClientFactory {

        //--- Fields ---
        private readonly TwitterClientConfig _config;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTime _dateTime;

        //--- Constructors ---
        public TwitterClientFactory(TwitterClientConfig config, IGuidGenerator guidGenerator, IDateTime dateTime) {
            _config = config;
            _guidGenerator = guidGenerator;
            _dateTime = dateTime;
        }

        //--- Methods ---
        public ITwitterClient NewTwitterClient(ITwitterConnection connection) {
            return new TwitterClient(_config, connection, _guidGenerator, _dateTime);
        }
    }
}
