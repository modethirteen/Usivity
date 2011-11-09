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
            var contacts = _data.GetContacts(UsivityContext.Current.User);
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
            var contact = _data.GetContact(context.GetParam<string>("contactid"), UsivityContext.Current.User);
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            var doc = GetContactXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
           
        }

        [DreamFeature("POST:contacts", "Create a new contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield PostContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contactDoc = GetRequestXml(request);
            var contact = GetContact(contactDoc);
            _data.SaveContact(contact);
            response.Return(DreamMessage.Ok(GetContactXml(contact)));
            yield break;
        }

        [DreamFeature("PUT:contacts/{contactid}", "Update contact information")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield UpdateContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contactDoc = GetRequestXml(request);
            var contact = GetContact(contactDoc, context.GetParam<string>("contactid"));
            _data.SaveContact(contact);
            response.Return(DreamMessage.Ok(GetContactXml(contact)));
            yield break;
        }

        [DreamFeature("DELETE:contacts/{contactid}", "Remove contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield RemoveContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        //--- Methods ---
        private Contact GetContact(XDoc contactDoc, string id = null) {
            var contact = !string.IsNullOrEmpty(id)
                ? _data.GetContact(id, UsivityContext.Current.User)
                : new Contact(UsivityContext.Current.User);
            contact.FirstName = contactDoc["firstname"].Contents;
            contact.LastName = contactDoc["lastname"].Contents;
            return contact;
        }

        private XDoc GetContactXml(Contact contact, string relation = null) {
            return contact.ToDocument(relation).Attr("href", _contactsUri.At(contact.Id));
        }
    }
}
