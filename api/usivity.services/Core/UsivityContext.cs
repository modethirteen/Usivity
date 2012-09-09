using System;
using MindTouch.Dream;
using Usivity.Entities;

namespace Usivity.Services.Core {

    public interface ICurrentContext {
        User User { get; set; }
        User.UserRole Role { get; set; }
        XUri ApiUri { get; set; }
        XUri UiUri { get; set; }
        XUri DefaultAvatarUri { get; set; }
    }

    public class UsivityContext : ICurrentContext, IDisposable {

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
                throw new DreamException("DreamContext does not contain a reference to UsivityContext");
            }
            return context;
        }

        //--- Properties ---
        public User User { get; set; }
        public User.UserRole Role { get; set; }
        public XUri ApiUri { get; set; }
        public XUri UiUri { get; set; }
        public XUri DefaultAvatarUri { get; set; }

        //--- Methods ---
        public void Dispose() {}
    }
}