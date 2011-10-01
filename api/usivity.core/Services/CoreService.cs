using System.Collections.Generic;
using log4net;
using MindTouch;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("Usivity Core API Service", "",
        SID = new[] { "sid://usivity.com/2011/01/core" }
    )]
    [DreamServiceConfig("mongodb/host", "string", "MongoDB hostname")]
    [DreamServiceConfig("mongodb/database", "string", "MongoDB database")]
    public class UsivityService : DreamService {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private Plug _openStream;

        //--- Properties ---
        public string MasterApiKey { get; private set; }

        //--- Methods ---
        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            _log.Debug("Setting Master API Key");
            MasterApiKey = config["apikey"].AsText ?? string.Empty;
            if(string.IsNullOrEmpty(MasterApiKey)) {
                throw new DreamException("Core apikey has not been set in startup config");
            }

            _log.Debug("Initializing current data session");
            var dataSessionConfig = new XDoc("config")
                .Elem("host", config["mongodb/host"].AsText)
                .Elem("database", config["mongodb/database"].AsText);
            UsivityDataSession.Initialize(dataSessionConfig);

            _log.Debug("Starting OpenStream service");
            XUri openStreamUri;
            if(config["uri.openstream"].IsEmpty) {
                yield return CreateService(
                    "openstream",
                    "sid://usivity.com/2011/01/openstream",
                    new XDoc("config")
                        .Elem("apikey", MasterApiKey)
                        .Elem("core.uri", Self.Uri)
                        .Elem("refresh", config["openstream/refresh"].AsText)
                        .EndAll(),
                    new Result<Plug>()
                );
                openStreamUri = Self.Uri.At("uri.openstream");
            } else {
                openStreamUri = config["uri.openstream"].AsUri;
            }
            _openStream = Plug.New(openStreamUri);
            result.Return();
        }
    }
}
