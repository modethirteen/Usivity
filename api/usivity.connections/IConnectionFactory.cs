﻿using Usivity.Entities;

namespace Usivity.Connections {

    public interface IConnectionFactory {

        //--- Methods ---
        IConnection NewConnection(Organization organization);
    }
}
