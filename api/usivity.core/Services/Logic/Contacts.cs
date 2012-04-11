using System.Linq;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Data.Entities;

namespace Usivity.Core.Services.Logic {

    public class Contacts : IContacts {

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataSession _data;
        private readonly IOrganizations _organizations;

        //--- Constructors ---
        public Contacts(IUsivityDataSession data, ICurrentContext context, IOrganizations organizations) {
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
            contact.FirstName = info["firstname"].Contents;
            contact.LastName = info["lastname"].Contents;
            contact.Avatar = info["uri.avatar"].AsUri;
            contact.Age = info["age"].Contents;
            contact.Gender = info["gender"].Contents;
            contact.Location = info["location"].Contents;
            contact.Phone = info["phone"].Contents;
            contact.Fax = info["fax"].Contents;
            contact.Address = info["address"].Contents;
            contact.City = info["city"].Contents;
            contact.State = info["state"].Contents;
            contact.Zip = info["zip"].Contents;
            contact.Email = info["email"].Contents;
            contact.Twitter = info["identity.twitter"].Contents;
            contact.Facebook = info["identity.facebook"].Contents;
            contact.LinkedIn = info["identity.linkedin"].Contents;
            contact.Google = info["identity.google"].Contents;
            contact.CompanyName = info["company/name"].Contents;
            contact.CompanyPhone = info["company/phone"].Contents;
            contact.CompanyFax = info["company/fax"].Contents;
            contact.CompanyAddress = info["company/address"].Contents;
            contact.CompanyCity = info["company/city"].Contents;
            contact.CompanyState = info["company/state"].Contents;
            contact.CompanyZip = info["company/zip"].Contents;
            contact.CompanyIndustry = info["company/industry"].Contents;
            contact.CompanyRevenue = info["company/revenue"].Contents;
            contact.CompanyCompetitors = info["company/competitors"].Contents;
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
