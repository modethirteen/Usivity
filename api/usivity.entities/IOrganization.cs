using Usivity.Entities.Types;

namespace Usivity.Entities {

    public interface IOrganization : IEntity {
        
        //--- Properties ---
        string Name { get; }

        //--- Methods ---
        string GetDefaultConnectionId(Source source);
    }
}
