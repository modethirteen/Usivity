using System.Collections.Generic;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities {

    public class Organization : IOrganization {

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; private set; }

        //--- Fields ---
        private IDictionary<Source, string> _defaultConnections;

        //--- Constructors ---
        private Organization(IGuidGenerator guidGenerator, string name) {
            Id = guidGenerator.GenerateNewObjectId();
            Name = name;
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "organization";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource).Attr("id", Id).Elem("name", Name);
        }

        public string GetDefaultConnectionId(Source source) {
            string connectionId;
            GetDefaultConnections().TryGetValue(source, out connectionId);
            return connectionId;
        }

        private IDictionary<Source, string> GetDefaultConnections() {
            return _defaultConnections ?? (_defaultConnections = new Dictionary<Source, string>());
        }
    }
}
