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
        private readonly AppSettings _appSettings;
        public MongoClient _client;


        public MongoDbService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

            _client = new MongoClient(_appSettings.MongoConnectionString);
            _mongoDb = _client.GetDatabase(_appSettings.MongoDatabase);
        }

        public IMongoCollection<Product> Product
        {
            get
            {
                return _mongoDb.GetCollection<Product>("Product");
            }
        }

        public IMongoCollection<ProductBrand> ProductBrand
        {
            get
            {
                return _mongoDb.GetCollection<ProductBrand>("ProductBrand");
            }
        }

        public IMongoCollection<ProductType> ProductType
        {
            get
            {
                return _mongoDb.GetCollection<ProductType>("ProductType");
            }
        }
    }

}