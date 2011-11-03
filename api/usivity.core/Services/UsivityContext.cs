using System;
using Usivity.Data.Entities;
using MindTouch.Dream;

namespace Usivity.Core.Services {

    public interface ICurrentUserContext {
        User User { get; set; }
        Organization Organization { get; set; }
    }

    public class UsivityContext : IDisposable {

        //--- Class Properties ---
        public static UsivityContext Current {
            get {
                return GetContext(DreamContext.Current);
            }
        }

        public static UsivityContext CurrentOrNull {
            get {
                var context = DreamContext.CurrentOrNull;
                return context == null ? null : context.GetState<UsivityContext>();
            }
        }

        //--- Class Methods ---
        public static UsivityContext GetContext(DreamContext dreamContext) {
            var context = dreamContext.GetState<UsivityContext>();
            if(context == null) {
                throw new DreamException("UsivityContext.Current is not set, because the current DreamContext does not contain a reference");
            }
            return context;
        }

        //--- Properties ---
        public User User { get; set; }
        public Organization Organization { get; set; }

        //--- Methods ---
        public void Dispose() {}
    }
}