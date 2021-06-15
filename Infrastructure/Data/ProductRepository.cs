using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderNeo4j;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Neo4jClient;

namespace Infrastructure.Data
{
    public class ProductRepository: IProductRepository
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IGraphClient _client;
        private readonly ICommentRepository _commentRepository;
        public ProductRepository(MongoDbService mongoDbService, IGraphClient client, ICommentRepository commentRepository)
        {
            _mongoDbService = mongoDbService;
            _client = client;
            _commentRepository = commentRepository;
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
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return;

            var type = await _mongoDbService.ProductType.Find(e => e._id == entity.ProductType._id).SingleOrDefaultAsync();
            if (type == null)
                return;

            var brand = await _mongoDbService.ProductBrand.Find(e => e._id == entity.ProductBrand._id).SingleOrDefaultAsync();
            if (brand == null)
                return;

            entity.ProductBrand.Name = brand.Name;
            entity.ProductType.Name = type.Name;

            await _mongoDbService.Product.InsertOneAsync(entity);


            //var result = await _client.Cypher.Merge("(pro:PRODUCT {ProductItemId: id})")
            //                                 .OnCreate()
            //                                 .Set("pro = {newProduct}")
            //                                 .WithParams(new { uuid = entity._id, newProduct = entity })
            //                                 .Return(pro => pro.As<ProductNeo4j>().uuid)
            //                                 .ResultsAsync;
            
            try
            {
                var tempProduct = new ProductNeo4j(entity);

                var result = await _client.Cypher.Create("(pro:PRODUCT)")
                                                 .Set("pro = $newProduct")
                                                 .WithParams(new { newProduct = tempProduct })
                                                 .Return(pro => pro.As<ProductNeo4j>().uuid)
                                                 .ResultsAsync;
                if (result == null)
                {
                    var filter = Builders<Product>.Filter.Eq(e => e._id, entity._id);
                    await _mongoDbService.Product.DeleteOneAsync(filter);
                }
            }
            catch(Exception ex)
            {
                var filter = Builders<Product>.Filter.Eq(e => e._id, entity._id);
                await _mongoDbService.Product.DeleteOneAsync(filter);
                return;
            }


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
            var delResult = await _mongoDbService.Product.DeleteOneAsync(filter);

            try
            {
                var result = await _client.Cypher.Match("(pro:PRODUCT)<-[r:BUY]-()")
                       .Where("pro.ProductItemId = $id")
                       .Set("pro.IsDeleted = 1")
                       .Delete("r")
                       .WithParams(new { id = entity._id })
                       .Return(pro => pro.As<ProductNeo4j>().uuid)
                       .ResultsAsync;

                var temp = result.Single();
                if (temp == null)
                {
                    await _mongoDbService.Product.InsertOneAsync(entity);
                    return;
                }

                //try
                //{
                    await _commentRepository.DeleteMany(entity._id);
                //}
            }
            catch (Exception ex)
            {
                await _mongoDbService.Product.InsertOneAsync(entity);
            }
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
            var backupProduct = await _mongoDbService.Product.Find(filter).SingleOrDefaultAsync();
            try
            {

                await _mongoDbService.Product.ReplaceOneAsync(filter, entity);

                var updatedProduct = new ProductNeo4j(entity);

                var result = await _client.Cypher.Match("(pro:PRODUCT)")
                                                   .Where("pro.ProductItemId = $id")
                                                   .Set("pro = $product")
                                                   .WithParams(new { id = entity._id, product = updatedProduct })
                                                   .Return(pro => pro.As<ProductNeo4j>().uuid)
                                                   .ResultsAsync;

                var temp = result.Single();
                if (temp == null)
                {
                    await _mongoDbService.Product.ReplaceOneAsync(filter, backupProduct);
                    return;
                }
            }
            catch (Exception ex)
            {
                await _mongoDbService.Product.ReplaceOneAsync(filter, backupProduct);
            }
        }

        public async Task<List<Product>> GetRecommendedProduct(string id)
        {
            try
            {
                await _client.ConnectAsync();
                if (!_client.IsConnected)
                    return null;

                var productList = await _client.Cypher.Match("(pro:PRODUCT)<-[:BUY]-(u:USER)-[:BUY]->(rec:PRODUCT)")
                                                      .Where<ProductNeo4j>(pro => pro.ProductItemId == "60ba6a410d59ab50c4c9617a")
                                                       .Return((rec) => new {
                                                           Recommendation = rec.As<ProductNeo4j>().ProductItemId,
                                                           Count = rec.Count()
                                                       })
                                                       .OrderByDescending("Count")
                                                       .Limit(5)
                                                       .ResultsAsync;
                var temp = productList.ToList();
                if (temp != null)
                {
                    if (temp.Count() != 0)
                    {
                        var idList = temp.Select(e => e.Recommendation).ToList();

                        var productInList = await _mongoDbService.Product.Find(e => idList.Contains(e._id)).ToListAsync();

                        return productInList;
                    }
                }
                return null;
            }
            catch(Exception ex)
            {
                return null;
            }       
        }

        private IFindFluent<Product, Product> ApplySpecification(ISpecification<Product> spec)
        {
            return SpecificationEvaluatorMongo<Product>.GetQuery(_mongoDbService.Product, spec);
        }
    }
}