using System.Linq;
using MindTouch.Dream;
using MindTouch.Xml;
using Usivity.Data;
using Usivity.Entities;
using Usivity.Entities.Types;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.Twitter;
using Usivity.Util;

namespace Usivity.Services.Core.Logic {

    public class Contacts : IContacts {

        class SourceAvatarInfo {
            
            //--- Properties ---
            public Source Source { get; set; }
            public XUri Avatar { get; set; }
        }

        //--- Fields ---
        private readonly ICurrentContext _context;
        private readonly IUsivityDataCatalog _data;
        private readonly IOrganizations _organizations;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IAvatarHelper _avatarHelper;

        //--- Constructors ---
        public Contacts(
            IGuidGenerator guidGenerator,
            IUsivityDataCatalog data,
            ICurrentContext context,
            IOrganizations organizations,
            IAvatarHelper avatarHelper
        ) {
            _context = context;
            _data = data;
            _organizations = organizations;
            _guidGenerator = guidGenerator;
            _avatarHelper = avatarHelper;
        }

        //--- Methods ---
        public Contact GetContact(string id) {
            return _data.Contacts.Get(id, _organizations.CurrentOrganization);
        }

        public Contact GetNewContact(XDoc info) {
            var contact = new Contact(_guidGenerator);
            UpdateContactInformation(contact, info);
            contact.AddOrganization(_organizations.CurrentOrganization);
            return contact;
        }

        public void UpdateContactInformation(Contact contact, XDoc info) {
            contact.FirstName = info["firstname"].AsText ?? contact.FirstName;
            contact.LastName = info["lastname"].AsText ?? contact.LastName;
            var avatar = info["uri.avatar"].AsText;
            if(!string.IsNullOrEmpty(avatar)) {
                XUri uri;
                XUri.TryParse(avatar, out uri);
                if(uri != null) {
                    contact.Avatar = uri;
                }
            }
            contact.Age = info["age"].AsText ?? contact.Age;
            contact.Gender = info["gender"].AsText ?? contact.Gender;
            contact.Location = info["location"].AsText ?? contact.Location;
            contact.Phone = info["phone"].AsText ?? contact.Phone;
            contact.Fax = info["fax"].AsText ?? contact.Fax;
            contact.Address = info["address"].AsText ?? contact.Address;
            contact.City = info["city"].AsText ?? contact.City;
            contact.State = info["state"].AsText ?? contact.State;
            contact.Zip = info["zip"].AsText ?? contact.Zip;
            contact.CompanyName = info["company/name"].AsText ?? contact.CompanyName;
            contact.CompanyPhone = info["company/phone"].AsText ?? contact.CompanyPhone;
            contact.CompanyFax = info["company/fax"].AsText ?? contact.CompanyFax;
            contact.CompanyAddress = info["company/address"].AsText ?? contact.CompanyAddress;
            contact.CompanyCity = info["company/city"].AsText ?? contact.CompanyCity;
            contact.CompanyState = info["company/state"].AsText ?? contact.CompanyState;
            contact.CompanyZip = info["company/zip"].AsText ?? contact.CompanyZip;
            contact.CompanyIndustry = info["company/industry"].AsText ?? contact.CompanyIndustry;
            contact.CompanyRevenue = info["company/revenue"].AsText ?? contact.CompanyRevenue;
            contact.CompanyCompetitors = info["company/competitors"].AsText ?? contact.CompanyCompetitors;

            // Email
            var newEmail = info["email"].AsText;
            var existingEmail = contact.GetIdentity(Source.Email);
            if(!string.IsNullOrEmpty(newEmail) && (existingEmail == null || existingEmail.Id != newEmail)) {
                contact.SetIdentity(Source.Email, EmailClient.NewIdentityFromEmailAddress(newEmail));
            }

            // Twitter
            var newTwitter = info["identity.twitter"].AsText;
            var existingTwitter = contact.GetIdentity(Source.Twitter);
            if(!string.IsNullOrEmpty(newTwitter) && (existingTwitter == null || existingTwitter.Name != newTwitter)) {
                contact.SetIdentity(Source.Twitter, TwitterClient.NewIdentityFromScreenName(newTwitter));
            }

            // Facebook
            var facebook = info["identity.facebook"].AsText;
            if(!string.IsNullOrEmpty(facebook)) {
                var facebookIdentity = new Identity {
                    Name = facebook
                };
                contact.SetIdentity(Source.Facebook, facebookIdentity);
            }

            // LinkedIn
            var linkedIn = info["identity.linkedin"].AsText;
            if(!string.IsNullOrEmpty(linkedIn)) {
                var linkedInIdentity = new Identity {
                    Name = linkedIn
                };
                contact.SetIdentity(Source.LinkedIn, linkedInIdentity);
            }

            // Google
            var google = info["identity.google"].AsText;
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
            var resource = "contact";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            var doc = new XDoc(resource)
                .Attr("id", contact.Id ?? "")
                .Attr("href", _context.ApiUri.At("contacts", contact.Id))
                .Elem("firstname", contact.FirstName ?? "")
                .Elem("lastname", contact.LastName ?? "")
                .Elem("email", contact.Email ?? "");
            if(contact.Avatar != null) {
                doc.Start("uri.avatar").Attr("default", false).Value(contact.Avatar.ToString()).End();
            }
            else {
                var avatarInfo = GetDefaultContactAvatar(contact);
                if(avatarInfo != null) {
                    doc.Start("uri.avatar")
                        .Attr("default", true)
                        .Attr("source", avatarInfo.Source.ToString().ToLowerInvariant())
                        .Value((avatarInfo.Avatar != null) ? avatarInfo.Avatar.ToString() : "")
                        .End();    
                }
                else {
                    doc.Elem("uri.avatar", "");
                }
            }
            return doc;
        }

        public XDoc GetContactVerboseXml(Contact contact, string relation = null) {
            var doc = GetContactXml(contact, relation)
                .Elem("age", contact.Age ?? "")
                .Elem("gender", contact.Gender ?? "")
                .Elem("location", contact.Location ?? "")
                .Elem("phone", contact.Phone ?? "")
                .Elem("fax", contact.Fax ?? "")
                .Elem("address", contact.Address ?? "")
                .Elem("city", contact.City ?? "")
                .Elem("state", contact.State ?? "")
                .Elem("zip", contact.Zip ?? "");            
            AppendIdentityXml(doc, "identity.twitter", contact.GetIdentity(Source.Twitter));
            AppendIdentityXml(doc, "identity.facebook", contact.GetIdentity(Source.Facebook));
            AppendIdentityXml(doc, "identity.linkedin", contact.GetIdentity(Source.LinkedIn));
            AppendIdentityXml(doc, "identity.google", contact.GetIdentity(Source.Google));
            var email = contact.GetIdentity(Source.Email);
            if(email != null) {
                doc.Start("identity.email")
                    .Attr("id", email.Id)
                    .Elem("name", email.Name ?? "")
                    .Elem("uri.avatar", _avatarHelper.GetGravatarUri(email.Id).ToString())
                .End();
            }
            else {
                doc.Elem("identity.email", "");
            }
            doc.Start("company")
                .Elem("name", contact.CompanyName ?? "")
                .Elem("phone", contact.CompanyPhone ?? "")
                .Elem("fax", contact.CompanyFax ?? "")
                .Elem("address", contact.CompanyAddress ?? "")
                .Elem("city", contact.CompanyCity ?? "")
                .Elem("state", contact.CompanyState ?? "")
                .Elem("zip", contact.CompanyZip ?? "")
                .Elem("industry", contact.CompanyIndustry ?? "")
                .Elem("revenue", contact.CompanyRevenue ?? "")
                .Elem("competitors", contact.CompanyCompetitors ?? "")
            .EndAll();
            return doc;
        }

        public void RemoveContact(Contact contact) {
            contact.RemoveOrganization(_organizations.CurrentOrganization.Id);
            SaveContact(contact);
        }

        private SourceAvatarInfo GetDefaultContactAvatar(Contact contact) {
            var twitter = contact.GetIdentity(Source.Twitter);
            if(twitter != null) {
                var avatar = _avatarHelper.GetAvatarUri(twitter);
                if(avatar != null) {
                    return new SourceAvatarInfo { Source = Source.Twitter, Avatar = avatar }; 
                }
            }
            var facebook = contact.GetIdentity(Source.Facebook);
            if(facebook != null) {
                var avatar = _avatarHelper.GetAvatarUri(facebook);
                if(avatar != null) {
                    return new SourceAvatarInfo { Source = Source.Facebook, Avatar = avatar }; 
                }
            }
            var linkedIn = contact.GetIdentity(Source.LinkedIn);
            if(linkedIn != null) {
                var avatar = _avatarHelper.GetAvatarUri(linkedIn);
                if(avatar != null) {
                    return new SourceAvatarInfo { Source = Source.LinkedIn, Avatar = avatar }; 
                }
            }
            var google = contact.GetIdentity(Source.Google);
            if(google != null) {
                var avatar = _avatarHelper.GetAvatarUri(google);
                if(avatar != null) {
                    return new SourceAvatarInfo { Source = Source.Google, Avatar = avatar }; 
                }
            }
            if(contact.Email != null) {
                var avatar = _avatarHelper.GetGravatarUri(contact.Email);
                if(avatar != null) {
                    return new SourceAvatarInfo { Source = Source.Email, Avatar = avatar }; 
                }
            }
            return null;
        }

        private void AppendIdentityXml(XDoc root, string name, Identity identity) {
            if(identity != null) {
                var avatar = _avatarHelper.GetAvatarUri(identity);
                root.AddAll(new XDoc(name)
                    .Attr("id", identity.Id)
                    .Elem("name", identity.Name ?? "")
                    .Elem("uri.avatar", (avatar != null) ? avatar.ToString() : "")
                    );    
            }
            else {
                root.Elem(name, "");
            }
        }
    }
}
