using System;
using Usivity.Entities.Types;

namespace Usivity.Entities.Connections {

    public interface IConnection : IEntity {

        //--- Properties ---
        string OrganizationId { get; }
        Source Source { get; }
        Identity Identity { get; set; }
        bool Active { get; }
        DateTime LastSearch { get; set; }
    }
}
