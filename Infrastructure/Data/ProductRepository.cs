using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderNeo4j;
using Core.Interfaces;
using Core.Specifications;
using MongoDB.Driver;
using Neo4jClient;

namespace Infrastructure.Data
{
    public class ProductRepository: IProductRepository
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IGraphClient _client;
        private readonly ICommentRepository _commentRepository;
        public ProductRepository(
            MongoDbService mongoDbService, 
            IGraphClient client, 
            ICommentRepository commentRepository      
            )
        {
            _mongoDbService = mongoDbService;
            _client = client;
            _commentRepository = commentRepository;
        }

        public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
        {
            var ret = await _mongoDbService.ProductBrands.Find(_ => true).ToListAsync();

            return ret;
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            var filter = Builders<Product>.Filter.Eq(e => e.Id, id);
            var ret = await _mongoDbService.Products.Find(filter).SingleOrDefaultAsync();
            return ret;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            var ret = await _mongoDbService.Products.Find(_ => true).ToListAsync();
            return ret;
        }

        public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
        {
            var ret = await _mongoDbService.ProductTypes.Find(_ => true).ToListAsync();
            return ret;
        }

        public async Task Add(Product entity)
        {
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return;

            var type = await _mongoDbService.ProductTypes.Find(e => e.Id == entity.ProductType.Id).SingleOrDefaultAsync();
            if (type == null)
                return;

            var brand = await _mongoDbService.ProductBrands.Find(e => e.Id == entity.ProductBrand.Id).SingleOrDefaultAsync();
            if (brand == null)
                return;

            entity.ProductBrand.Name = brand.Name;
            entity.ProductType.Name = type.Name;

            await _mongoDbService.Products.InsertOneAsync(entity);


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
                    var filter = Builders<Product>.Filter.Eq(e => e.Id, entity.Id);
                    await _mongoDbService.Products.DeleteOneAsync(filter);
                }
            }
            catch(Exception ex)
            {
                var filter = Builders<Product>.Filter.Eq(e => e.Id, entity.Id);
                await _mongoDbService.Products.DeleteOneAsync(filter);
                return;
            }


        }

        public async Task<int> CountAsync(ISpecification<Product> spec)
        {
            var listProduct =  await ApplySpecification(spec).ToListAsync();
            if (listProduct != null) return listProduct.Count;
            return 0;
        }

        public async Task Delete(Product entity)
        {
            var filter = Builders<Product>.Filter.Eq(e => e.Id, entity.Id);
            var delResult = await _mongoDbService.Products.DeleteOneAsync(filter);

            try
            {
                var result = await _client.Cypher.Match("(pro:PRODUCT)<-[r:BUY]-()")
                       .Where("pro.ProductItemId = $id")
                       .Set("pro.IsDeleted = 1")
                       .Delete("r")
                       .WithParams(new { id = entity.Id })
                       .Return(pro => pro.As<ProductNeo4j>().uuid)
                       .ResultsAsync;

                var temp = result.Single();
                if (temp == null)
                {
                    await _mongoDbService.Products.InsertOneAsync(entity);
                    return;
                }

                //try
                //{
                    await _commentRepository.DeleteMany(entity.Id);
                //}
            }
            catch (Exception ex)
            {
                await _mongoDbService.Products.InsertOneAsync(entity);
            }
        }

        public async Task<Product> GetByIdAsync(string _id)
        {
            var filter = Builders<Product>.Filter.Eq(e => e.Id, _id);
            var ret = await _mongoDbService.Products.Find(filter).SingleOrDefaultAsync();
            return ret;
        }

        public async Task<Product> GetEntityWithSpec(ISpecification<Product> spec)
        {
            return await ApplySpecification(spec).SingleOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Product>> ListAllAsync()
        {
            var ret = await _mongoDbService.Products.Find(_ => true).ToListAsync();
            return ret;
        }

        public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task Update(Product entity)
        {
            var filter = Builders<Product>.Filter.Eq(e => e.Id, entity.Id);
            var backupProduct = await _mongoDbService.Products.Find(filter).SingleOrDefaultAsync();
            try
            {

                await _mongoDbService.Products.ReplaceOneAsync(filter, entity);

                var updatedProduct = new ProductNeo4j(entity);

                var result = await _client.Cypher.Match("(pro:PRODUCT)")
                                                   .Where("pro.ProductItemId = $id")
                                                   .Set("pro = $product")
                                                   .WithParams(new { id = entity.Id, product = updatedProduct })
                                                   .Return(pro => pro.As<ProductNeo4j>().uuid)
                                                   .ResultsAsync;

                var temp = result.Single();
                if (temp == null)
                {
                    await _mongoDbService.Products.ReplaceOneAsync(filter, backupProduct);
                    return;
                }
            }
            catch (Exception ex)
            {
                await _mongoDbService.Products.ReplaceOneAsync(filter, backupProduct);
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

                        var productInList = await _mongoDbService.Products.Find(e => idList.Contains(e.Id)).ToListAsync();

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
            return SpecificationEvaluatorMongo<Product>.GetQuery(_mongoDbService.Products, spec);
        }
        public async Task<List<Product>> GetAllProductAsync(ProductSpecParams param)
        {
            var result  = await _mongoDbService.Products.Find( f => true).ToListAsync();
            var listProduct = result.Where(x =>
                (string.IsNullOrEmpty(param.Search) || x.Name.ToLower().Contains(param.Search))
                && (param.BrandName == "all"  || x.ProductBrand.Name.ToLower() == param.BrandName)
                && (param.TypeName == "all" || x.ProductType.Name.ToLower() == param.TypeName)).ToList();
            return listProduct;
        }
    }
}