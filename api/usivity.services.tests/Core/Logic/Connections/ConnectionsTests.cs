﻿using Moq;
using Usivity.Data;
using Usivity.Services.Clients.Email;
using Usivity.Services.Clients.Twitter;
using Usivity.Services.Core;
using Usivity.Services.Core.Logic;
using Usivity.Util;

namespace Usivity.Tests.Services.Core.Logic.Connections {
    using Connections = Usivity.Services.Core.Logic.Connections;

    public class ConnectionsTests {

        //--- Fields ---
        protected Mock<IUsivityDataCatalog> _dataMock;
        protected Mock<ICurrentContext> _contextMock;
        protected Mock<IOrganizations> _organizationsMock;
        protected Mock<IGuidGenerator> _guidGeneratorMock;
        protected Mock<ITwitterClientFactory> _twitterClientFactoryMock;
        protected Mock<IEmailClientFactory> _emailClientFactoryMock;

        //--- Methods ---
        protected void SetUp() {

            // mock dependencies
            _dataMock = new Mock<IUsivityDataCatalog>();
            _contextMock = new Mock<ICurrentContext>();
            _organizationsMock = new Mock<IOrganizations>();
            _guidGeneratorMock = new Mock<IGuidGenerator>();
            _twitterClientFactoryMock = new Mock<ITwitterClientFactory>();
            _emailClientFactoryMock = new Mock<IEmailClientFactory>();
        }

        protected Connections GetConnections() {
            return new Connections(
                _guidGeneratorMock.Object,
                _dataMock.Object,
                _contextMock.Object,
                _organizationsMock.Object,
                _emailClientFactoryMock.Object,
                _twitterClientFactoryMock.Object
                );
        }
    }
}