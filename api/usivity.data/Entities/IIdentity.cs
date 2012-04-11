using System;

namespace Usivity.Data.Entities {
    
    public interface IIdentity {
        
        //--- Properties ---
        string Id { get; }
        string Name { get; }
        Uri Avatar { get; }
    }
}
