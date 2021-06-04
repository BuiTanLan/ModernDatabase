using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Infrastructure.Data
{
    public class ProductRepository: IProductRepository
    {
        private readonly MongoDbService _mongoDbService;
        public ProductRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
        {
            var ret = await _mongoDbService.ProductBrand.Find(_ => true).ToListAsync();

            return ret;
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            var filter = Builders<Product>.Filter.Eq(e => e._id, id);
            var ret = await _mongoDbService.Product.Find(filter).SingleOrDefaultAsync();
            return ret;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            var ret = await _mongoDbService.Product.Find(_ => true).ToListAsync();
            return ret;
        }

        public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
        {
            var ret = await _mongoDbService.ProductType.Find(_ => true).ToListAsync();
            return ret;
        }

        public async Task Add(Product entity)
        {
            await _mongoDbService.Product.InsertOneAsync(entity);
        }

        public async Task<int> CountAsync(ISpecification<Product> spec)
        {
            var temp1 = ApplySpecification(spec);
            var temp = await temp1.ToListAsync();
            return temp.Count();
        }

        public async Task Delete(Product entity)
        {
            var filter = Builders<Product>.Filter.Eq(e => e._id, entity._id);
            await _mongoDbService.Product.DeleteOneAsync(filter);
        }

        public async Task<Product> GetByIdAsync(string _id)
        {
            var filter = Builders<Product>.Filter.Eq(e => e._id, _id);
            var ret = await _mongoDbService.Product.Find(filter).SingleOrDefaultAsync();
            return ret;
        }

        public async Task<Product> GetEntityWithSpec(ISpecification<Product> spec)
        {
            return await ApplySpecification(spec).SingleOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Product>> ListAllAsync()
        {
            var ret = await _mongoDbService.Product.Find(_ => true).ToListAsync();
            return ret;
        }

        public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task Update(Product entity)
        {
            var filter = Builders<Product>.Filter.Eq(e => e._id, entity._id);
            await _mongoDbService.Product.ReplaceOneAsync(filter,entity);
        }

        private IFindFluent<Product, Product> ApplySpecification(ISpecification<Product> spec)
        {
            return SpecificationEvaluatorMongo<Product>.GetQuery(_mongoDbService.Product, spec);
        }
    }
}