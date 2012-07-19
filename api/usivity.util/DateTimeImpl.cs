using System;

namespace Usivity.Util {

    public class DateTimeImpl : IDateTime {

        //--- Fields ---
        private readonly DateTime _utcNow;

        //--- Constructors ---
        public DateTimeImpl() : this(DateTime.UtcNow) { }

        public DateTimeImpl(DateTime now) {
            _utcNow = now.ToSafeUniversalTime();
        }

        //--- Properties ---
        public DateTime UtcNow { get { return _utcNow; } }
        public DateTime UtcToday { get { return _utcNow.Date; } }
    }
}
