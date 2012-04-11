using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Data.Connections;

namespace Usivity.Data.Entities {

    public class Subscription : IEntity {

        //--- Class Properties ---
        public enum SubscriptionLanguage {
            English
        }

        public static IDictionary<string, SubscriptionLanguage> SubscriptionLanguages =
            new Dictionary<string, SubscriptionLanguage> {
                { "en", SubscriptionLanguage.English }
        };

        //--- Properties ---
        public string Id { get; set; }
        public string OrganizationId { get; private set; }
        public IEnumerable<string> Constraints { get; private set; }
        public SubscriptionLanguage Language { get; private set; }
        public bool Active { get; set; }

        //--- Fields ---
        private IDictionary<SourceType, Uri> _uris;

        //--- Constructors ---
        public Subscription(Organization organization, IEnumerable<string> constraints, SubscriptionLanguage language) {
            Id = UsivityDataSession.GenerateEntityId(this);
            Language = language;
            OrganizationId = organization.Id;
            Constraints = constraints;
            Active = true;
            _uris = new Dictionary<SourceType, Uri>();
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

        public void SetSourceUri(SourceType source, Uri uri) {
            _uris[source] = uri;
        }

        public Uri GetSourceUri(SourceType source) {
            Uri uri;
            _uris.TryGetValue(source, out uri);
            return uri;
        }
    }
}
