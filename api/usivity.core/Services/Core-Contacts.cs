using System;
using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [DreamFeature("GET:contacts", "Get all contacts claimed by user")]
        public Yield GetContacts(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }

        [DreamFeature("GET:contacts/{contactid}", "Get contact by id")]
        [DreamFeatureParam("contactid", "string", "Contact id")]
        public Yield GetContact(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            throw new NotImplementedException();
        }
    }
}
