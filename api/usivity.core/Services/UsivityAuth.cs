using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MindTouch.Dream;
using MindTouch.Web;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    
    class UsivityAuth {
  
        //--- Constants ---
        internal const string AUTHTOKEN_HEADERNAME = "X-Authtoken";
        internal const string AUTHTOKEN_COOKIENAME = "authtoken";
        internal const string AUTHTOKEN_PATTERN = @"^(?<id>([\d])+)_(?<ts>([\d]){18})_(?<hash>.+)$";

        //--- Class Fields ---
        private static readonly Regex _authTokenRegex =
            new Regex(AUTHTOKEN_PATTERN, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        //--- Class Methods ---
        public static UsivityAuth Factory(string salt) {
            return new UsivityAuth(salt);
        }

        //--- Fields ---
        private readonly string _salt;

        //--- Constructors ---
        private UsivityAuth(string salt) {
            _salt = salt;
        }

        //--- Methods ---
        public User GetAuthenticatedUser(DreamMessage request) {
            string authToken = null;
            if(request.HasCookies) {
                var authCookie = DreamCookie.GetCookie(request.Cookies, AUTHTOKEN_COOKIENAME);
                if(authCookie != null && !authCookie.Expired) {
                    authToken = authCookie.Value;
                }
            }
            if(string.IsNullOrEmpty(authToken)) {
                authToken = request.Headers[AUTHTOKEN_HEADERNAME];    
            }
            if(authToken == null) {
                return null;
            }
            var m = _authTokenRegex.Match(authToken);
            if(!m.Success) {
                return null;
            }
            return null;
        }

        public string GenerateAuthToken(User user) {
            var token = string.Format("{0}_{1}", user.Id, DateTime.UtcNow.ToUniversalTime().Ticks);
            var validate = string.Format("{0}.{1}.{2}", token, user.Password ?? string.Empty, _salt);
            var md5 = MD5.Create();
            var hash = new Guid(md5.ComputeHash(Encoding.Default.GetBytes(validate))).ToString("N");
            return token + "_" + hash;
        }
    }
}
