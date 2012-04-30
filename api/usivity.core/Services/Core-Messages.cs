using System;
using System.Collections.Generic;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;
using Usivity.Core.Services.Logic;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Core.Services {
    using Yield = IEnumerator<IYield>;

    public partial class CoreService {

        //--- Features ---
        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:messages", "Get messages from stream")]
        [DreamFeatureParam("limit", "int?", "Max messages to receive ranging 1 - 100 (default: 100)")]
        [DreamFeatureParam("offset", "int?", "Receive messages starting with offset (default: none)")]
        [DreamFeatureParam("start", "string?", "Oldest message timestamp to receive in format: yyyy-MM-ddTHH:mm:ssZ (default: now)")]
        [DreamFeatureParam("end", "string?", "Newest message timestamp to receive in format: yyyy-MM-ddTHH:mm:ssZ (default: +1 hour)")]
        [DreamFeatureParam("source", "{email,twitter}?", "Filter by source")]
        internal Yield GetMessages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var count = context.GetParam("limit", 100);
            var offset = context.GetParam("offset", 0);
            var start = context.GetParam("start", null);
            var end = context.GetParam("end", null);
            DateTime startTime, endTime;
            if(!string.IsNullOrEmpty(end)) {
                try {
                    endTime = DateTime.Parse(end);
                } catch {
                    throw new DreamException("\"end\" is not a valid datetime parameter");
                }    
            } else {
                endTime = DateTime.UtcNow;
            }
            if(!string.IsNullOrEmpty(start)) {
                try {
                    startTime = DateTime.Parse(start);
                } catch {
                    throw new DreamException("\"start\" is not a valid datetime parameter");
                }    
            } else {
                startTime = endTime.AddHours(-1);
            }
            var messages = Resolve<IMessages>(context);
            var sourceFilter = context.GetParam("source", null);
            XDoc doc;
            if(string.IsNullOrEmpty(sourceFilter)) {
                doc = messages.GetMessageStreamXml(startTime, endTime, count, offset);
            }
            else {
                Source source;
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
                doc = messages.GetMessageStreamXml(startTime, endTime, count, offset, source); 
            }
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [UsivityFeatureAccess(User.UserRole.Member)]
        [DreamFeature("GET:messages/{messageid}", "Get message")]
        [DreamFeatureParam("messageid", "string", "Message id")]
        internal Yield GetMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            var messages = Resolve<IMessages>(context);
            var message = messages.GetMessage(context.GetParam<string>("messageid"));
            if(message == null) {
                response.Return(DreamMessage.NotFound("The requested message could not be located"));
                yield break;
            }
            var doc = messages.GetMessageVerboseXml(message);
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
