﻿using System;
using Usivity.Data.Entities;
using MindTouch.Dream;

namespace Usivity.Core.Services {

    public interface ICurrentContext {
        User User { get; set; }
        XUri ApiUri { get; set; }
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
        public XUri ApiUri { get; set; }

        //--- Methods ---
        public void Dispose() {}
    }
}