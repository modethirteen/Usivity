using System;
using Usivity.Entities.Types;

namespace Usivity.Entities.Connections {

    public interface IConnection : IEntity {

        //--- Properties ---
        string OrganizationId { get; }
        Source Source { get; }
        Identity Identity { get; set; }
        DateTime Created { get; set; }
    }
}
