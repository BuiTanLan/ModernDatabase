using Core.Entities;

namespace Core.Specifications
{
    public class ProductsWithTypesAndBrandsSpecification : BaseSpecification<Product>
    {
        public ProductsWithTypesAndBrandsSpecification(ProductSpecParams productParams)
            : base(x =>
                string.IsNullOrWhiteSpace(productParams.Search) || x.Name.ToLower().Contains(productParams.Search))
                // (productParams.BrandName == "all" || x.ProductBrand.Name.ToLower() == productParams.BrandName) &&
                // (productParams.TypeName == "all" || x.ProductType.Name.ToLower() == productParams.TypeName))
        {

            AddOrderBy(x => x.Name);
            ApplyPaging(productParams.PageSize * (productParams.PageIndex - 1), productParams.PageSize); 
            if (!string.IsNullOrEmpty(productParams.Sort))
            {
                switch (productParams.Sort)
                {
                    case "asc":
                        AddOrderBy(p => p.Price);
                        break;
                    case "desc":
                        AddOrderByDescending(p => p.Price);
                        break;
                    default:
                        AddOrderBy(p => p.Name);
                        break;
                }
            }
        }
        public ProductsWithTypesAndBrandsSpecification(string id) : base(x => x.Id == id)
        {
            //AddIncluded(x => x.ProductType);
            //AddIncluded(x => x.ProductBrand);
            //AddIncluded(x => x.Photos);
        }
    }
}