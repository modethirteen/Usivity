using Moq;
using Usivity.Data;
using Usivity.Services.Core;
using Usivity.Services.Core.Logic;

namespace Usivity.Tests.Services.Core.Logic.Connections {
    using Connections = Usivity.Services.Core.Logic.Connections;

    public class ConnectionsTests {

        //--- Fields ---
        protected Mock<IUsivityDataCatalog> _dataMock;
        protected Mock<ICurrentContext> _contextMock;
        protected Mock<IOrganizations> _organizationsMock;

        //--- Methods ---
        protected void SetUp() {

            // mock dependencies
            _dataMock = new Mock<IUsivityDataCatalog>();
            _contextMock = new Mock<ICurrentContext>();
            _organizationsMock = new Mock<IOrganizations>();
        }

        protected Connections GetConnections() {
            return new Connections(_dataMock.Object, _contextMock.Object, _organizationsMock.Object);
        }
    }
}
