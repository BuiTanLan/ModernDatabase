using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;

namespace Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Add(Product entity)
        {
            await _productRepository.Add(entity);
        }

        public async Task<int> CountAsync(ISpecification<Product> spec)
        {
            return await _productRepository.CountAsync(spec);
        }

        public async Task Delete(Product entity)
        {
            await _productRepository.Delete(entity);
        }

        public async Task<Product> GetByIdAsync(string id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> GetEntityWithSpec(ISpecification<Product> spec)
        {
            return await _productRepository.GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Product>> ListAllAsync()
        {
            return await _productRepository.ListAllAsync();
        }

        public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec)
        {
            return await _productRepository.ListAsync(spec);
        }

        public async Task Update(Product entity)
        {
             await _productRepository.Update(entity);
        }

        public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
        {
            return await _productRepository.GetProductBrandsAsync();
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            return await _productRepository.GetProductByIdAsync(id);
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            return await _productRepository.GetProductsAsync();
        }

        public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
        {
            return await _productRepository.GetProductTypesAsync();
        }

        public async Task<List<Product>> GetRecommendedProduct(string id)
        {
            return await _productRepository.GetRecommendedProduct(id);
        }

        public async Task<Tuple<List<Product>, int>> GetAllProductAsync(ProductSpecParams param)
        {
            var result =  await _productRepository.GetAllProductAsync(param);
            IOrderedEnumerable<Product> orderedEnumerable;
            switch (param.Sort)
            {
                case "asc":
                    orderedEnumerable = result.OrderBy(p => p.Price);
                    break;
                case "desc":
                    orderedEnumerable = result.OrderByDescending(p => p.Price);

                    break;
                default:
                    orderedEnumerable = result.OrderBy(p => p.Name);
                    break;
            }

            var paging = orderedEnumerable.Skip((param.PageIndex - 1) * param.PageSize).Take(param.PageSize).ToList();
            return new Tuple<List<Product>, int>(paging, result.Count);
        }
    }
}