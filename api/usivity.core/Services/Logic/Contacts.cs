using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Connections.Twitter;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services.Logic {

    public class Contacts : IContacts {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganizations _organizations;

        //--- Constructors ---
        public Contacts(IUsivityDataCatalog data, ICurrentContext context, IOrganizations organizations) {
            _context = context;
            _data = data;
            _organizations = organizations;
        }

        //--- Methods ---
        public Contact GetContact(string id) {
            return _data.Contacts.Get(id, _organizations.CurrentOrganization);
        }

        public Contact GetNewContact(XDoc info) {
            var contact = new Contact();
            UpdateContactInformation(contact, info);
            contact.AddOrganization(_organizations.CurrentOrganization);
            return contact;
        }

        public void UpdateContactInformation(Contact contact, XDoc info) {
            contact.FirstName = info["firstname"].Contents ?? contact.FirstName;
            contact.LastName = info["lastname"].Contents ?? contact.LastName;
            var avatar = info["uri.avatar"].Contents;
            if(!string.IsNullOrEmpty(avatar)) {
                XUri uri;
                XUri.TryParse(avatar, out uri);
                if(uri != null) {
                    contact.Avatar = uri;
                }
            }
            contact.Age = info["age"].Contents ?? contact.Age;
            contact.Gender = info["gender"].Contents ?? contact.Gender;
            contact.Location = info["location"].Contents ?? contact.Location;
            contact.Phone = info["phone"].Contents ?? contact.Phone;
            contact.Fax = info["fax"].Contents ?? contact.Fax;
            contact.Address = info["address"].Contents ?? contact.Address;
            contact.City = info["city"].Contents ?? contact.City;
            contact.State = info["state"].Contents ?? contact.State;
            contact.Zip = info["zip"].Contents ?? contact.Zip;
            contact.CompanyName = info["company/name"].Contents ?? contact.CompanyName;
            contact.CompanyPhone = info["company/phone"].Contents ?? contact.CompanyPhone;
            contact.CompanyFax = info["company/fax"].Contents ?? contact.CompanyFax;
            contact.CompanyAddress = info["company/address"].Contents ?? contact.CompanyAddress;
            contact.CompanyCity = info["company/city"].Contents ?? contact.CompanyCity;
            contact.CompanyState = info["company/state"].Contents ?? contact.CompanyState;
            contact.CompanyZip = info["company/zip"].Contents ?? contact.CompanyZip;
            contact.CompanyIndustry = info["company/industry"].Contents ?? contact.CompanyIndustry;
            contact.CompanyRevenue = info["company/revenue"].Contents ?? contact.CompanyRevenue;
            contact.CompanyCompetitors = info["company/competitors"].Contents ?? contact.CompanyCompetitors;

            // Email
            var email = info["email"].Contents;
            if(!string.IsNullOrEmpty(email)) {
                var emailIdentity = new Identity {
                    Id = email
                };
                contact.SetIdentity(Source.Email, emailIdentity);
            }

            // Twitter
            var twitter = info["identity.twitter"].Contents;
            if(!string.IsNullOrEmpty(twitter)) {
                var twitterIdentity = TwitterPublicConnection.GetInstance().GetIdentity(twitter);
                contact.SetIdentity(Source.Twitter, twitterIdentity);
            }

            // Facebook
            var facebook = info["identity.facebook"].Contents;
            if(!string.IsNullOrEmpty(facebook)) {
                var facebookIdentity = new Identity {
                    Name = facebook
                };
                contact.SetIdentity(Source.Facebook, facebookIdentity);
            }

            // LinkedIn
            var linkedIn = info["identity.linkedin"].Contents;
            if(!string.IsNullOrEmpty(linkedIn)) {
                var linkedInIdentity = new Identity {
                    Name = linkedIn
                };
                contact.SetIdentity(Source.LinkedIn, linkedInIdentity);
            }

            // Google Plus
            var google = info["identity.google"].Contents;
            if(!string.IsNullOrEmpty(google)) {
                var googleIdentity = new Identity {
                    Name = google
                };
                contact.SetIdentity(Source.Google, googleIdentity);
            }
        }

        public void SaveContact(Contact contact) {
            _data.Contacts.Save(contact);
        }

        public XDoc GetContactsXml() {
            var contacts = _data.Contacts.Get(_organizations.CurrentOrganization);
            var doc = new XDoc("contacts")
                .Attr("count", contacts.Count())
                .Attr("href", _context.ApiUri.At("contacts"));
            foreach(var contact in contacts) {
                doc.Add(GetContactXml(contact));
            }
            doc.EndAll();
            return doc;
        }

        public XDoc GetContactXml(Contact contact, string relation = null) {
            return contact.ToDocument(relation)
                .Attr("href", _context.ApiUri.At("contacts", contact.Id));
        }

        public XDoc GetContactVerboseXml(Contact contact, string relation = null) {
            return contact.ToDocumentVerbose(relation)
                .Attr("href", _context.ApiUri.At("contacts", contact.Id));
        }

        public void RemoveContact(Contact contact) {
            contact.RemoveOrganization(_organizations.CurrentOrganization.Id);
            SaveContact(contact);
        } 
    }
}
