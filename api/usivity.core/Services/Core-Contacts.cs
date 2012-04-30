using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Core.Services.Logic;
using Usivity.Entities;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:contacts", "Get all contacts")]
        internal Yield GetContacts(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var doc = Resolve<IContacts>(context).GetContactsXml();
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:contacts/{contactid}", "Get contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        internal Yield GetContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("POST:contacts", "Create a new contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        internal Yield PostContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = Resolve<IContacts>(context);
            var contact = contacts.GetNewContact(GetRequestXml(request));
            contacts.SaveContact(contact);
            var doc = contacts.GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("PUT:contacts/{contactid}", "Update contact information")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        internal Yield UpdateContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var contacts = Resolve<IContacts>(context);
            var contact = contacts.GetContact(context.GetParam<string>("contactid"));
            if(contact == null) {
                response.Return(DreamMessage.NotFound("The requested contact could not be located"));
                yield break;
            }
            contacts.UpdateContactInformation(contact, GetRequestXml(request));
            contacts.SaveContact(contact);
            var doc = contacts.GetContactVerboseXml(contact);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("DELETE:contacts/{contactid}", "Remove contact")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        internal Yield RemoveContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
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
