using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Usivity.Connections;
using Usivity.Entities;

namespace Usivity.Tests.Services.Core.Logic.Connections {

    [TestFixture]
    public class GetDefaultConnection : ConnectionsTests {

        //--- Methods ---
        [SetUp]
        public void SetUp() {
            base.SetUp();
        }

        [Test]
        public void Can_get_valid_default_connection() {
            const string id = "123";
            var source = Utils.GetRandomSource();

            // mock current organization
            var currentOrganizationMock = new Mock<IOrganization>();
            currentOrganizationMock.Setup(o => o.Id).Returns("foo");
            currentOrganizationMock.Setup(o => o.GetDefaultConnectionId(source)).Returns(id);
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganizationMock.Object);

            // create mock connection for current organization with id
            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(c => c.Id).Returns(id);
            mockConnection.Setup(c => c.OrganizationId).Returns(currentOrganizationMock.Object.Id);

            // attach foo to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(mockConnection.Object);

            // get default connection by source
            var connections = GetConnections();
            var connection = connections.GetDefaultConnection(source);
            Assert.IsNotNull(connection);
        }

        [Test]
        public void Fetching_default_connection_returns_correct_connection() {
            const string id = "123";
            var source = Utils.GetRandomSource();

            // mock current organization
            var currentOrganizationMock = new Mock<IOrganization>();
            currentOrganizationMock.Setup(o => o.Id).Returns("foo");
            currentOrganizationMock.Setup(o => o.GetDefaultConnectionId(source)).Returns(id);
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganizationMock.Object);

            // create mock connection for current organization with id
            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(c => c.Id).Returns(id);
            mockConnection.Setup(c => c.OrganizationId).Returns(currentOrganizationMock.Object.Id);

            // attach foo to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(mockConnection.Object);

            // get default connection by source
            var connections = GetConnections();
            var connection = connections.GetDefaultConnection(source);
            Assert.AreEqual(id, connection.Id);
        }

        [Test]
        public void Fetching_default_connection_when_not_defined_falls_back_to_active_connection_with_correct_source() {
            const string id = "123";
            var source = Utils.GetRandomSource();

            // mock current organization
            var currentOrganizationMock = new Mock<IOrganization>();
            currentOrganizationMock.Setup(o => o.Id).Returns("foo");
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganizationMock.Object);

            // create active source mock connection for current organization with id
            var mockConnection = new Mock<IConnection>();
            mockConnection.Setup(c => c.Id).Returns(id);
            mockConnection.Setup(c => c.Source).Returns(source);
            mockConnection.Setup(c => c.Active).Returns(true);
            mockConnection.Setup(c => c.OrganizationId).Returns(currentOrganizationMock.Object.Id);

            // attach foo to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(mockConnection.Object);
            _dataMock.Setup(data => data.Connections.Get(currentOrganizationMock.Object, source))
                .Returns(new List<IConnection> { mockConnection.Object });

            // get default connection by source
            var connections = GetConnections();
            var connection = connections.GetDefaultConnection(source);
            Assert.AreEqual(id, connection.Id);
        }
    }
}
