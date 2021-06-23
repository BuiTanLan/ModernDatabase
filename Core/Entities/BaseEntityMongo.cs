using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Core.Entities
{
    public class BaseEntityMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}