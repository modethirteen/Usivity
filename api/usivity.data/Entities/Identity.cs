using System;

namespace Usivity.Data.Entities {

    public class Identity : IIdentity {

        //--- Properties ---
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri Avatar { get; set; }
    }
}
