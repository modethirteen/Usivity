using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:contacts", "Get all contacts")]
        protected Yield GetContacts(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = _data.Contacts.Get(UsivityContext.Current.Organization);
            var doc = new XDoc("contacts")
                .Attr("count", contacts.Count())
                .Attr("href", _contactsUri);
            foreach(var contact in contacts) {
                doc.Add(GetContactXml(contact));
            }
            doc.EndAll();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:contacts/{contactid}", "Get contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield GetContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contact = _data.Contacts.Get(context.GetParam<string>("contactid"), UsivityContext.Current.Organization);
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            var doc = GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("POST:contacts", "Create a new contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield PostContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contactDoc = GetRequestXml(request);
            var contact = GetContact(contactDoc);
            contact.AddOrganization(UsivityContext.Current.Organization);
            _data.Contacts.Save(contact);
            response.Return(DreamMessage.Ok(GetContactVerboseXml(contact)));
            yield break;
        }

        [DreamFeature("PUT:contacts/{contactid}", "Update contact information")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield UpdateContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contactDoc = GetRequestXml(request);
            var contact = GetContact(contactDoc, context.GetParam<string>("contactid"));
            _data.Contacts.Save(contact);
            response.Return(DreamMessage.Ok(GetContactVerboseXml(contact)));
            yield break;
        }

        [DreamFeature("DELETE:contacts/{contactid}", "Remove contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield RemoveContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var organization = UsivityContext.Current.Organization;
            var contact = _data.Contacts.Get(context.GetParam<string>("contactid"), organization);
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            contact.RemoveOrganization(organization.Id);
            _data.Contacts.Save(contact);
            response.Return(DreamMessage.Ok());
            yield break;
        
        }

        //--- Methods ---
        private Contact GetContact(XDoc contactDoc, string id = null) {
            var contact = !string.IsNullOrEmpty(id) ? _data.Contacts.Get(id, UsivityContext.Current.Organization) : new Contact();
            contact.FirstName = contactDoc["firstname"].Contents;
            contact.LastName = contactDoc["lastname"].Contents;
            contact.Avatar = contactDoc["uri.avatar"].AsUri;
            contact.Age = contactDoc["age"].Contents;
            contact.Gender = contactDoc["gender"].Contents;
            contact.Location = contactDoc["location"].Contents;
            contact.Phone = contactDoc["phone"].Contents;
            contact.Fax = contactDoc["fax"].Contents;
            contact.Address = contactDoc["address"].Contents;
            contact.City = contactDoc["city"].Contents;
            contact.State = contactDoc["state"].Contents;
            contact.Zip = contactDoc["zip"].Contents;
            contact.Email = contactDoc["email"].Contents;
            contact.Twitter = contactDoc["identity.twitter"].Contents;
            contact.Facebook = contactDoc["identity.facebook"].Contents;
            contact.LinkedIn = contactDoc["identity.linkedin"].Contents;
            contact.Google = contactDoc["identity.google"].Contents;
            contact.CompanyName = contactDoc["company/name"].Contents;
            contact.CompanyPhone = contactDoc["company/phone"].Contents;
            contact.CompanyFax = contactDoc["company/fax"].Contents;
            contact.CompanyAddress = contactDoc["company/address"].Contents;
            contact.CompanyCity = contactDoc["company/city"].Contents;
            contact.CompanyState = contactDoc["company/state"].Contents;
            contact.CompanyZip = contactDoc["company/zip"].Contents;
            contact.CompanyIndustry = contactDoc["company/industry"].Contents;
            contact.CompanyRevenue = contactDoc["company/revenue"].Contents;
            contact.CompanyCompetitors = contactDoc["company/competitors"].Contents;
            return contact;
        }

        private XDoc GetContactVerboseXml(Contact contact, string relation = null) {
            var doc = contact.ToDocumentVerbose(relation).Attr("href", _contactsUri.At(contact.Id));
            doc.Start("messages");
            var conversations = _data.GetMessageStream(UsivityContext.Current.Organization)
                .GetConversations(contact);
            foreach(var message in conversations) {
                doc.AddAll(GetMessageXml(message))
                    .AddAll(GetMessageChildrenXml(message));
            }
            return doc;
        }

        private XDoc GetContactXml(Contact contact, string relation = null) {
            return contact.ToDocument(relation).Attr("href", _contactsUri.At(contact.Id));
        }
    }
}
