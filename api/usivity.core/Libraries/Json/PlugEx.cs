using MindTouch.Dream;
using MindTouch.Tasking;

namespace Usivity.Core.Libraries.Json {

    public static class PlugEx {

        public static Result<JDoc> Get(this Plug plug, Result<JDoc> result) {
            plug.Invoke(Verb.GET, DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(r => {
                if(r.HasException) {
                    result.Throw(r.Exception);
                }
                else if(!r.Value.IsSuccessful) {
                    result.Throw(new DreamResponseException(r.Value));
                }
                else {
                    var j = new JDoc(r.Value.ToText());
                    result.Return(j);
                }
            });
            return result;
        }
    }
}