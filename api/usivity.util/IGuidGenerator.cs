namespace Usivity.Util {

    public interface IGuidGenerator {

        //--- Methods ---
        string GenerateNewObjectId(string salt = null);
    }
}
