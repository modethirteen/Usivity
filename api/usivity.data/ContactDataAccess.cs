﻿using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Usivity.Entities;

namespace Usivity.Data {

    public class ContactDataAccess : IContactDataAccess {

        //--- Fields ---
        private readonly MongoCollection _db;

        //--- Constructors ---
        public ContactDataAccess(MongoDatabase db) {
            _db = db.GetCollection<Contact>("contacts");
        }
   
        //--- Methods ---
        public IEnumerable<Contact> Get(IOrganization organization) {
            return _db.FindAs<Contact>(Query.EQ("_organizations", organization.Id));
        }

        public Contact Get(IMessage message) {
            var identity = "_identities." + (int)message.Source;
            var query = Query.And(
                Query.EQ(identity + "._id", message.Author.Id)
                );
            return _db.FindOneAs<Contact>(query);
        }

        public Contact Get(string id, IOrganization organization) {
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
