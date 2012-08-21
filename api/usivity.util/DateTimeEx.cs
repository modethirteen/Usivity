using System;
using System.Globalization;

namespace Usivity.Util {

    public static class DateTimeEx {

        public static string ToISO8601String(this DateTime dateTime) {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }
    }
}
