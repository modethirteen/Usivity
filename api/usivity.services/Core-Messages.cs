using System;
using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Services {
    using Core.Logic;
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:messages", "Get messages from stream")]
        [DreamFeatureParam("limit", "int?", "Max messages to receive ranging 1 - 100 (default: 100)")]
        [DreamFeatureParam("offset", "int?", "Receive messages starting with offset (default: none)")]
        [DreamFeatureParam("start", "int?", "Milliseconds back in time to stream (default: 3600000)")]
        [DreamFeatureParam("source", "{email,twitter}?", "Filter by source")]
        internal Yield GetMessages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var count = context.GetParam("limit", 100);
            var offset = context.GetParam("offset", 0);
            var start = context.GetParam("start", 3600000);
            var startTime = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(start));
            var messages = Resolve<IMessages>(context);
            var sourceFilter = context.GetParam("source", null);
            Source? source = null;
            if(!string.IsNullOrEmpty(sourceFilter)) {
                switch(sourceFilter.ToLowerInvariant()) {
                    case "twitter":
                        source = Source.Twitter;
                        break;
                    case "email":
                        source = Source.Email;
                        break;
                    default:
                        throw new DreamBadRequestException(string.Format("\"{0}\" is not a valid source filter parameter", sourceFilter));
                }       
            }
            var doc = messages.GetMessageStreamXml(startTime, DateTime.UtcNow, count, offset, source); 
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:messages/{messageid}", "Get message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        [DreamFeatureParam("children", "{tree,flat}?", "Child message hiearchy mode (default: tree)")]
        internal Yield GetMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var messages = Resolve<IMessages>(context);
            var message = messages.GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var tree = context.GetParam("children", "tree").ToLowerInvariant() == "tree";
            var doc = messages.GetMessageVerboseXml(message, tree);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("DELETE:messages/{messageid}", "Delete message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        internal Yield DeleteMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var messages = Resolve<IMessages>(context);
            var message = messages.GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            messages.DeleteMessage(message);
            response.Return(DreamMessage.Ok());
            yield break;
        }
     
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("POST:messages/{messageid}", "Post message in reply")]
        [DreamFeatureParam("message", "string", "Message id to reply to")]
        internal Yield PostMessageReply(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var messages = Resolve<IMessages>(context);
            var message = messages.GetMessage(context.GetParam<string>("messageid"));          
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            DreamMessage msg;
            try {
                var doc = messages.PostReply(message, request.ToText());
                msg = DreamMessage.Ok(doc);
            }
            catch(DreamAbortException e) {
                msg = e.Response; 
            }
            response.Return(msg);
            yield break;
        }
    }
}
