﻿using System;
using System.Collections.Generic;
using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public class Contact : IEntity {

        //--- Fields ---
        private IDictionary<string, SourceIdentity> _identities;
        private List<string> _conversations;

        //--- Properties ---
        public string Id { get; private set; }
        public string ClaimedByUserId { get; set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        //--- Constructors ---
        public Contact(string firstName, string lastName, User claimingUser) {
            Id = UsivityDataSession.GenerateEntityId(this);
            FirstName = firstName;
            LastName = lastName;
            ClaimedByUserId = claimingUser.Id;
            _identities = new Dictionary<string, SourceIdentity>();
            _conversations = new List<string>();
        }

        //--- Methods ---
        public XDoc ToDocument(string relation = null) {
            var resource = "contact";
            if(!string.IsNullOrEmpty(relation)) {
                resource += "." + relation;
            }
            return new XDoc(resource)
                .Attr("id", Id)
                .Elem("firstname", FirstName)
                .Elem("lastname", LastName);
        }

        public void SetSourceIdentity(string source, SourceIdentity identity) {
            _identities[source] = identity;
        }

        public SourceIdentity GetSourceIdentity(string source) {
            return _identities.TryGetValue(source, null);
        }

        public IDictionary<string, SourceIdentity> GetSourceIdentities() {
            return _identities;
        }

        public IEnumerable<string> GetConversations() {
            return _conversations;
        }

        public void AddConversationMessageId(string messageId) {
            _conversations.Add(messageId);
        }
        
        public void RemoveConversationMessageId(string messageId) {
            _conversations.Remove(messageId);
        }
    }
}
