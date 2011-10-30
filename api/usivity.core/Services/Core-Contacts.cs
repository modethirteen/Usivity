using System;
using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Data.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:contacts", "Get all contacts claimed by user")]
        public Yield GetContacts(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        [DreamFeature("GET:contacts/{contactid}", "Get contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        public Yield GetContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        [DreamFeature("POST:contacts", "Create a new contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        public Yield PostContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        [DreamFeature("PUT:contacts/{contactid}", "Update a contact information")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        public Yield UpdateContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        private XDoc GetContactXml(Contact contact, string relation = null) {
            return contact.ToDocument(relation).Attr("href", _contactsUri.At(contact.Id));
        }
    }
}
