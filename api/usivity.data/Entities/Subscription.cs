using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class Subscription : IEntity {

        //--- Properties ---
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Source { get; private set; }
        public IEnumerable<string> Constraints { get; set; }
        public Uri Uri { get; set; }
        public bool Active { get; set; }

        //--- Constructors ---
        public Subscription(string source) {
            Id = UsivityDataSession.GenerateEntityId(this);
            Source = source;
            Active = true;
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "subscription";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", Id)
                .Elem("active", Active ? "true" : "false")
                .Elem("constraints", string.Join(",", Constraints.ToArray()))
                .EndAll();   
        }
    }
}
