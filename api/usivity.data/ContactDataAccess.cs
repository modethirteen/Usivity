﻿using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Usivity.Data {
    using Entities;

    public class ContactDataAccess : IContactDataAccess {

        //--- Class Methods ---
        public static void RegisterClassMap() {
            BsonClassMap.RegisterClassMap<Contact>(cm => {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.MapField("_identities");
                cm.MapField("_organizations");
            });
        }

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public ContactDataAccess(MongoDatabase db) {
            _db = db.GetCollection<Contact>("contacts");
        }
   
        //--- Methods ---
        public IEnumerable<Contact> Get(Organization organization) {
            return _db.FindAs<Contact>(Query.EQ("_organizations", organization.Id));
        }

        public Contact Get(Message message) {
            var identity = "_identities." + message.Source;
            var query = Query.And(
                Query.Exists(identity, true),
                Query.EQ(identity + "._id", message.Author.Id)
                );
            return _db.FindOneAs<Contact>(query);
        }

        public Contact Get(string id, Organization organization) {
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("_organizations", organization.Id)
            );
            return _db.FindOneAs<Contact>(query);
        }

        public void Save(Contact contact) {
            _db.Save(contact, SafeMode.True);
        }
    }
}
