using System;
using System.Collections.Generic;
using MindTouch.Dream;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities {

    public class Contact : IEntity {

        //--- Fields ---
        private Dictionary<Source, Identity> _identities;
        private List<string> _organizations;

        //--- Properties ---
        public string Id { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public XUri Avatar { get; set; }
        public string Email { get {
            var email = GetIdentity(Source.Email);
            return (email != null) ? email.Id : null;
        } }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyFax { get; set;}
        public string CompanyAddress { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public string CompanyZip { get; set; }
        public string CompanyIndustry { get; set; }
        public string CompanyRevenue { get; set; }
        public string CompanyCompetitors { get; set; }
        public IEnumerable<KeyValuePair<Source, Identity>> Identities { get { return _identities; } }

        //--- Constructors ---
        public Contact(IGuidGenerator guidGenerator) {
            Id = guidGenerator.GenerateNewObjectId();
            _identities = new Dictionary<Source, Identity>();
            _organizations = new List<string>();
        }

        //--- Methods ---
        public void SetIdentity(Source source, Identity identity) {
            _identities[source] = identity;
        }

        public Identity GetIdentity(Source source) {
            return _identities.TryGetValue(source, null);
        }

        public void AddOrganization(IOrganization organization) {
            if(_organizations.Contains(organization.Id)) {
                return;  
            }
            _organizations.Add(organization.Id); 
        }

        public void RemoveOrganization(string organizationId) {
            if(!_organizations.Contains(organizationId)) {
                return;  
            }
            _organizations.Remove(organizationId);
        }
    }
}
