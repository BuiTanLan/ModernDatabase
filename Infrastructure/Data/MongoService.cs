using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;
namespace Infrastructure.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _mongoDb;
        public MongoDbService(IOptions<AppSettings> appSettings)
        {
            var mongoSetting = appSettings.Value;
            var client = new MongoClient(mongoSetting.MongoConnectionString);
            _mongoDb = client.GetDatabase(mongoSetting.MongoDatabase);
        }
        public IMongoCollection<Product> Products => _mongoDb.GetCollection<Product>("Product");

        public IMongoCollection<ProductBrand> ProductBrands => _mongoDb.GetCollection<ProductBrand>("ProductBrand");

        public IMongoCollection<ProductType> ProductTypes => _mongoDb.GetCollection<ProductType>("ProductType");
    }

}