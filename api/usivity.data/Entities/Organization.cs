using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class Organization : IEntity {

        //--- Properties ---
        public string Id { get; private set; }
        public string Name { get; private set; }

        //--- Constructors ---
        public Organization(string name) {
            Id = UsivityDataSession.GenerateEntityId(this);
            Name = name;
        }

        public XDoc ToDocument(string relation = null) {
            var resource = "organization";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource).Attr("id", Id).Elem("name", Name);
        }
    }
}
