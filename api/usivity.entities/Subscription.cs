using System;
using System.Collections.Generic;
using System.Linq;
using MindTouch.Xml;
using Usivity.Entities.Types;
using Usivity.Util;

namespace Usivity.Entities {

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
        private IDictionary<Source, Uri> _uris;

        //--- Constructors ---
        public Subscription(IOrganization organization, IEnumerable<string> constraints, SubscriptionLanguage language) {
            Id = GuidGenerator.CreateUnique();
            Language = language;
            OrganizationId = organization.Id;
            Constraints = constraints;
            Active = true;
            _uris = new Dictionary<Source, Uri>();
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

        public void SetSourceUri(Source source, Uri uri) {
            _uris[source] = uri;
        }

        public Uri GetSourceUri(Source source) {
            Uri uri;
            _uris.TryGetValue(source, out uri);
            return uri;
        }
    }
}
