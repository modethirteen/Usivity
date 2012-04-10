using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Core.Services.Logic;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:contacts", "Get all contacts")]
        protected Yield GetContacts(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IContacts>(context).GetContactsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:contacts/{contactid}", "Get contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield GetContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = Resolve<IContacts>(context);
            var contact = contacts.GetContact(context.GetParam<string>("contactid"));
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            var doc = contacts.GetContactVerboseXml(contact)
                .AddAll(Resolve<IMessages>(context).GetConversationsXml(contact));
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("POST:contacts", "Create a new contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield PostContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = Resolve<IContacts>(context);
            var contact = contacts.GetNewContact(GetRequestXml(request));
            contacts.SaveContact(contact);
            var doc = contacts.GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("PUT:contacts/{contactid}", "Update contact information")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield UpdateContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = Resolve<IContacts>(context);
            var contact = contacts.GetContact(context.GetParam<string>("contactid"));
            contacts.UpdateContactInformation(contact, GetRequestXml(request));
            contacts.SaveContact(contact);
            var doc = contacts.GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("DELETE:contacts/{contactid}", "Remove contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        protected Yield RemoveContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = Resolve<IContacts>(context);
            var contact = contacts.GetContact(context.GetParam<string>("contactid")); 
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            contacts.RemoveContact(contact);
            response.Return(DreamMessage.Ok());
            yield break;
        }
    }
}
