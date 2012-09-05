using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Util;

namespace Usivity.Entities {

    public class User : IUser {

        //--- Constants ---
        public const string ANONYMOUS_USER = "Anonymous";

        //--- Class Properties ---
        public enum UserRole {
            None = 0,
            Member = 10,
            Admin = 20,
            Owner = 30
        }

        //--- Class Methods ---
        public static User GetAnonymousUser() {
            return new User {
                Id = int.MinValue.ToString(),
                Name = ANONYMOUS_USER,
                IsAnonymous = true
            };
        }

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string CurrentOrganization { get; set; }
        public bool IsAnonymous { get; private set; }

        //--- Fields ---
        private Dictionary<string, UserRole> _organizations = new Dictionary<string, UserRole>();

        //--- Constructors ---
        public User(IGuidGenerator guidGenerator, string name, string email) {
            Id = guidGenerator.GenerateNewObjectId();
            EmailAddress = email;
            Name = name;
        }

        private User() {}

        //--- Methods ---
        public UserRole GetOrganizationRole(IOrganization organization) {
            return (organization == null) ? UserRole.None : _organizations.TryGetValue(organization.Id, UserRole.None);
        }

        public void SetOrganizationRole(IOrganization organization, UserRole role) {
            _organizations[organization.Id] = role;
        }

        public IEnumerable<string> GetOrganizationIds() {
            return _organizations.Select(organization => organization.Key).ToList();
        }
    }
}
