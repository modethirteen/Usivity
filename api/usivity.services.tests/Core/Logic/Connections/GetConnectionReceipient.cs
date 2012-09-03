using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Usivity.Entities;
using Usivity.Entities.Connections;
using Usivity.Entities.Types;

namespace Usivity.Tests.Services.Core.Logic.Connections {

    [TestFixture]
    public class GetConnectionReceipient : ConnectionsTests {

        //--- Methods ---
        [SetUp]
        public void SetUp() {
            base.SetUp();
        }

        [Test]
        public void Can_get_connection_from_message_receipient() {
            const string id = "123";
            var source = Utils.GetRandomSource();

            // mock current organization
            var currentOrganization = Utils.GetOrganization();
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganization);

            // create connection for current organization with id, source, and identity
            var identity = new Identity { Id = Utils.GetUniqueId() };
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.Id).Returns(id);
            connection.Setup(c => c.Source).Returns(source);
            connection.Setup(c => c.Identity).Returns(identity);
            connection.Setup(c => c.OrganizationId).Returns(currentOrganization.Id);

            // attach connection to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(connection.Object);
            _dataMock.Setup(data => data.Connections.Get(currentOrganization, null))
                .Returns(new List<IConnection> { connection.Object });

            // create mock message with reply to identity id matching connection identity id
            var message = new Mock<IMessage>();
            message.Setup(m => m.SourceInReplyToIdentityId).Returns(identity.Id);
            message.Setup(m => m.Source).Returns(source);

            // get connection by message receipient
            var connections = GetConnections();
            var receipientConnection = connections.GetConnectionReceipient(message.Object);
            Assert.IsNotNull(receipientConnection);
        }

        [Test]
        public void Fetching_connection_from_message_receipient_returns_correct_connection() {
            const string id = "123";
            var source = Utils.GetRandomSource();

            // mock current organization
            var currentOrganization = Utils.GetOrganization();
            _organizationsMock.Setup(o => o.CurrentOrganization)
                .Returns(currentOrganization);

            // create connection for current organization with id, source, and identity
            var identity = new Identity { Id = Utils.GetUniqueId() };
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.Id).Returns(id);
            connection.Setup(c => c.Source).Returns(source);
            connection.Setup(c => c.Identity).Returns(identity);
            connection.Setup(c => c.OrganizationId).Returns(currentOrganization.Id);

            // attach connection to mock data catalog
            _dataMock.Setup(data => data.Connections.Get(id)).Returns(connection.Object);
            _dataMock.Setup(data => data.Connections.Get(currentOrganization, null))
                .Returns(new List<IConnection> { connection.Object });

            // create mock message with reply to identity id matching connection identity id
            var message = new Mock<IMessage>();
            message.Setup(m => m.SourceInReplyToIdentityId).Returns(identity.Id);
            message.Setup(m => m.Source).Returns(source);

            // get connection by message receipient
            var connections = GetConnections();
            var receipientConnection = connections.GetConnectionReceipient(message.Object);
            Assert.AreEqual(id, receipientConnection.Id);
        }
    }
}
