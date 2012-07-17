namespace Usivity.Entities.Types {

    public enum Source {
        Twitter,
        LinkedIn,
        Facebook,
        Google,
        Email
    }

    public static class SourceUtil {

        //--- Class Methods ---
        public static int GetSourceValue(this Source source) {
            return (int) source;
        }
    }
}
