using System;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;

namespace Usivity.Util {

    public class GuidGenerator : IGuidGenerator {

        //--- Methods ---
        public string GenerateNewObjectId(string salt = null) {
            var id = ObjectId.GenerateNewId().ToString();
            if(salt != null) {
                var md5 = MD5.Create();
                var salted = string.Format("{0}{1}", id, salt);
                id = new Guid(md5.ComputeHash(Encoding.Default.GetBytes(salted))).ToString("N");
            }
            return id;
        }   
    }
}
