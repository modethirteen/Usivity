using Moq;
using NUnit.Framework;
using Usivity.Connections;
using Usivity.Entities;

namespace Usivity.Tests.Services.Core.Logic.Connections {

    [TestFixture]
    public class GetConnection : ConnectionsTests {

        //--- Methods ---
        [SetUp]
        public void SetUp() {
            base.SetUp();
        }

        [Test]
        public void Can_get_valid_connection_by_id() {
            const string id = "123";

            // mock current organization
            var currentOrganizationMock = new Mock<IOrganization>();
            currentOrganizationMock.Setup(o => o.Id).Returns("foo");
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganizationMock.Object);

            // create mock connection for current organization with id
            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(c => c.Id).Returns(id);
            mockConnection.Setup(c => c.OrganizationId).Returns(currentOrganizationMock.Object.Id);

            // attach foo to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(mockConnection.Object);

            // get connection by id 123
            var connections = GetConnections();
            var connection = connections.GetConnection(id);
            Assert.IsNotNull(connection);
        }

        [Test]
        public void Fetching_connection_by_id_returns_correct_connection() {
            const string id = "123";

            // mock current organization
            var currentOrganizationMock = new Mock<IOrganization>();
            currentOrganizationMock.Setup(o => o.Id).Returns("foo");
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganizationMock.Object);

            // create mock connection for current organization with id
            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(c => c.Id).Returns(id);
            mockConnection.Setup(c => c.OrganizationId).Returns(currentOrganizationMock.Object.Id);

            // attach foo to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(mockConnection.Object);

            // get connection by id 123
            var connections = GetConnections();
            var connection = connections.GetConnection(id);
            Assert.AreEqual(id, connection.Id);
        }

        [Test]
        public void Cannot_get_connection_by_id_from_non_current_organization() {
            const string id = "123";

            // mock current organization
            var currentOrganizationMock = new Mock<IOrganization>();
            currentOrganizationMock.Setup(o => o.Id).Returns("foo");
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganizationMock.Object);

            // create mock connection for current organization with id
            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(c => c.Id).Returns(id);
            mockConnection.Setup(c => c.OrganizationId).Returns("bar");

            // attach foo to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(mockConnection.Object);

            // get connection by id 123
            var connections = GetConnections();
            var connection = connections.GetConnection(id);
            Assert.IsNull(connection);
        }
    }
}
