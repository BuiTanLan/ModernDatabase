using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Core.Entities
{
    public class ProductType : BaseEntityMongo
    {
        public string Name { get; set; }
    }
}