using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities.OrderAggregate;
using Core.Entities;
using Core.Specifications;

namespace Core.Interfaces
{
    public interface IProductService
    {
        Task<Product> GetProductByIdAsync(string id);
        Task<IReadOnlyList<Product>> GetProductsAsync();
        Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync();
        Task<IReadOnlyList<ProductType>> GetProductTypesAsync();
        Task<Product> GetByIdAsync(string id);
        Task<IReadOnlyList<Product>> ListAllAsync();
        Task<Product> GetEntityWithSpec(ISpecification<Product> spec);
        Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec);
        Task<int> CountAsync(ISpecification<Product> spec);
        Task Add(Product entity);
        Task Update(Product entity);
        Task Delete(Product entity);
        Task<List<Product>> GetRecommendedProduct(string id);

    }
}