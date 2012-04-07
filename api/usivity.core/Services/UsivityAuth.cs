using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MindTouch.Dream;
using MindTouch.Web;
using Usivity.Data;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    
    class UsivityAuth {
  
        //--- Constants ---
        internal const string AUTHTOKEN_HEADERNAME = "X-Authtoken";
        internal const string AUTHTOKEN_COOKIENAME = "authtoken";
        internal const string AUTHTOKEN_PATTERN = @"^(?<id>([\d]).*)_(?<ts>([\d]){18})_(?<hash>.*)$";

        //--- Class Fields ---
        private static readonly Regex _authTokenRegex =
            new Regex(AUTHTOKEN_PATTERN, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        //--- Class Methods ---
        public static UsivityAuth Factory(string salt, XUri cookieUri, UsivityDataSession data) {
            return new UsivityAuth(salt, cookieUri, data);
        }

        //--- Fields ---
        private readonly string _salt;
        private readonly UsivityDataSession _data;
        private readonly MD5 _md5;
        private readonly XUri _cookieUri;
    
        //--- Constructors ---
        private UsivityAuth(string salt, XUri cookieUri, UsivityDataSession data) {
            _salt = salt;
            _data = data;
            _md5 = MD5.Create();
            _cookieUri = cookieUri;
        }

        //--- Methods ---
        public string GetSaltedPassword(string password) {
            var salted = string.Format("{0}.{1}", password, _salt);
            return new Guid(_md5.ComputeHash(Encoding.Default.GetBytes(salted))).ToString("N");
        }

        public string GetAuthToken(DreamMessage request) {
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
            return authToken;
        }
        
        public User GetAuthenticatedUser(string authToken = null) {
            if(string.IsNullOrEmpty(authToken)) {
                return null;
            }
            var m = _authTokenRegex.Match(authToken);
            if(!m.Success) {
                return null;
            }
            var user = _data.Users.Get(m.Groups["id"].Value);
            if(user != null) {
                var validationToken = GenerateAuthTokenHelper(user, m.Groups["ts"].Value);
                if(authToken != validationToken) {
                    return null;
                }
            }
            return user;
        }

        public string GenerateAuthToken(User user) {
            return user == null ? null : GenerateAuthTokenHelper(user, DateTime.UtcNow.ToUniversalTime().Ticks.ToString());
        }

        public DreamCookie GetAuthCookie(string authToken, DateTime expiration) {
            return DreamCookie.NewSetCookie(AUTHTOKEN_COOKIENAME, authToken, _cookieUri, expiration);
        }

        private string GenerateAuthTokenHelper(User user, string ticks) {
            var token = string.Format("{0}_{1}", user.Id, ticks);
            var validate = string.Format("{0}.{1}.{2}", token, user.Password ?? string.Empty, _salt);
            var hash = new Guid(_md5.ComputeHash(Encoding.Default.GetBytes(validate))).ToString("N");
            return token + "_" + hash;
        }
    }
}
