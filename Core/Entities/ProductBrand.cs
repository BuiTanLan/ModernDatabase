using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Core.Entities
{
    public class ProductBrand : BaseEntityMongo
    {
        public string Name { get; set; }
    }
}