using Moq;
using NUnit.Framework;
using Usivity.Entities.Connections;

namespace Usivity.Tests.Services.Core.Logic.Connections {

    [TestFixture]
    class GetConnection : ConnectionsTests {

        //--- Methods ---
        [SetUp]
        public void SetUp() {
            base.SetUp();
        }

        [Test]
        public void Can_get_valid_connection_by_id() {
            const string id = "123";

            // mock current organization
            var currentOrganization = Utils.GetOrganization();
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganization);

            // create mock connection for current organization with id
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.Id).Returns(id);
            connection.Setup(c => c.OrganizationId).Returns(currentOrganization.Id);

            // attach connection to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(connection.Object);

            // get connection by id 123
            var connections = GetConnections();
            Assert.IsNotNull(connections.GetConnection(id));
        }

        [Test]
        public void Fetching_connection_by_id_returns_correct_connection() {
            const string id = "123";

            // mock current organization
            var currentOrganization = Utils.GetOrganization();
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganization);

            // create mock connection for current organization with id
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.Id).Returns(id);
            connection.Setup(c => c.OrganizationId).Returns(currentOrganization.Id);

            // attach connection to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(connection.Object);

            // get connection by id 123
            var connections = GetConnections();
            Assert.AreEqual(id, connections.GetConnection(id).Id);
        }

        [Test]
        public void Cannot_get_connection_by_id_from_non_current_organization() {
            const string id = "123";

            // mock current organization
            var currentOrganization = Utils.GetOrganization();
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganization);

            // create mock connection for current organization with id
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.Id).Returns(id);
            connection.Setup(c => c.OrganizationId).Returns("bar");

            // attach connection to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(connection.Object);

            // get connection by id 123
            var connections = GetConnections();
            Assert.IsNull(connections.GetConnection(id));
        }
    }
}
