using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Core.Entities
{
    public class BaseEntityNeo4j
    {
        public string Id { get; set; }
    }
}