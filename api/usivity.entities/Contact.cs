using System;
using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities {

    public class Contact : IEntity {

        //--- Fields ---
        private IDictionary<Source, Identity> _identities;
        private IList<string> _organizations;

        //--- Properties ---
        public string Id { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Uri Avatar { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public Identity Email {
            get {
                Identity email;
                _identities.TryGetValue(Source.Email, out email);
                return email;
            }
        }
        public Identity Twitter {
            get {
                Identity twitter;
                _identities.TryGetValue(Source.Twitter, out twitter);
                return twitter;
            }
        }
        public Identity Google {
            get {
                Identity google;
                _identities.TryGetValue(Source.Google, out google);
                return google;
            }
        }
        public Identity Facebook {
            get {
                Identity facebook;
                _identities.TryGetValue(Source.Facebook, out facebook);
                return facebook;
            }
        }
        public Identity LinkedIn {
            get {
                Identity linkedIn;
                _identities.TryGetValue(Source.LinkedIn, out linkedIn);
                return linkedIn;
            }
        }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set;}
        public string CompanyFax { get; set;}
        public string CompanyAddress { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public string CompanyZip { get; set; }
        public string CompanyIndustry { get; set; }
        public string CompanyRevenue { get; set;}
        public string CompanyCompetitors { get; set;}

        //--- Constructors ---
        public Contact(IGuidGenerator guidGenerator) {
            Id = guidGenerator.GenerateNewObjectId();
            _identities = new Dictionary<Source, Identity>();
            _organizations = new List<string>();
        }

        //--- Methods ---
        public XDoc ToDocumentVerbose(string relation = null) {
            return ToDocument(relation)
                .Elem("age", Age ?? "")
                .Elem("gender", Gender ?? "")
                .Elem("location", Location ?? "")
                .Elem("phone", Phone ?? "")
                .Elem("fax", Fax ?? "")
                .Elem("address", Address ?? "")
                .Elem("city", City ?? "")
                .Elem("state", State ?? "")
                .Elem("zip", Zip ?? "")
                .Elem("identity.twitter", Twitter != null ? Twitter.Name : "")
                .Elem("identity.facebook", Facebook != null ? Facebook.Name : "")
                .Elem("identity.linkedin", LinkedIn != null ? LinkedIn.Name : "")
                .Elem("identity.google", Google != null ? Google.Name : "")
                .Start("company")
                    .Elem("name", CompanyName ?? "")
                    .Elem("phone", CompanyPhone ?? "")
                    .Elem("fax", CompanyFax ?? "")
                    .Elem("address", CompanyAddress ?? "")
                    .Elem("city", CompanyCity ?? "")
                    .Elem("state", CompanyState ?? "")
                    .Elem("zip", CompanyZip ?? "")
                    .Elem("industry", CompanyIndustry ?? "")
                    .Elem("revenue", CompanyRevenue ?? "")
                    .Elem("competitors", CompanyCompetitors ?? "")
                .EndAll();
        }

        public XDoc ToDocument(string relation = null) {
            var resource = "contact";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", Id ?? "")
                .Elem("firstname", FirstName ?? "")
                .Elem("lastname", LastName ?? "")
                .Elem("email", Email != null ? Email.Id : "")
                .Elem("uri.avatar", Avatar != null ? Avatar.ToString() : "");
        }

        public void SetIdentity(Source source, Identity identity) {
            _identities[source] = identity;
        }

        public Identity GetSourceIdentity(Source source) {
            return _identities.TryGetValue(source, null);
        }

        public IDictionary<Source, Identity> GetSourceIdentities() {
            return _identities;
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
