using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;

namespace Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _proRepo;

        public ProductService(IProductRepository proRepo)
        {
            _proRepo = proRepo;
        }

        public async Task Add(Product entity)
        {
            await _proRepo.Add(entity);
        }

        public async Task<int> CountAsync(ISpecification<Product> spec)
        {
            return await _proRepo.CountAsync(spec);
        }

        public async Task Delete(Product entity)
        {
            await _proRepo.Delete(entity);
        }

        public async Task<Product> GetByIdAsync(string id)
        {
            return await _proRepo.GetByIdAsync(id);
        }

        public async Task<Product> GetEntityWithSpec(ISpecification<Product> spec)
        {
            return await _proRepo.GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Product>> ListAllAsync()
        {
            return await _proRepo.ListAllAsync();
        }

        public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec)
        {
            return await _proRepo.ListAsync(spec);
        }

        public async Task Update(Product entity)
        {
             await _proRepo.Update(entity);
        }

        public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
        {
            return await _proRepo.GetProductBrandsAsync();
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            return await _proRepo.GetProductByIdAsync(id);
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            return await _proRepo.GetProductsAsync();
        }

        public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
        {
            return await _proRepo.GetProductTypesAsync();
        }

        public async Task<List<Product>> GetRecommendedProduct(string id)
        {
            return await _proRepo.GetRecommendedProduct(id);
        }
    }
}