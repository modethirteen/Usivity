using System;
using MindTouch.Dream;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Usivity.Data.Serializers {

    public class XUriSerializer : BsonBaseSerializer, IUsivityDataSerializer {

        //--- Class Fields ---
        private static bool _isRegistered;

        //--- Methods ---
        public void RegisterSerializer() {
            if(!_isRegistered) {
                BsonSerializer.RegisterSerializer(typeof(XUri), new XUriSerializer());
                _isRegistered = true;
            }
        }

        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options) {
            VerifyTypes(nominalType, actualType, typeof(XUri));
            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType) {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;
                case BsonType.String:
                    return new XUri(bsonReader.ReadString());
                default:
                    throw new Exception(string.Format("Cannot deserialize XUri from BsonType {0}.", bsonType));
            }
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options) {
            if(value == null) {
                bsonWriter.WriteNull();
            }
            else {
                bsonWriter.WriteString(((XUri)value).ToString());
            }
        }
    }
}
