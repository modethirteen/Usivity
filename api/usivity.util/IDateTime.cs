using System;

namespace Usivity.Util {

    public interface IDateTime {

        //--- Properties ---
        DateTime UtcNow { get; }
        DateTime UtcToday { get; }
    }
}
