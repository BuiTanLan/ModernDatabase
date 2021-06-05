using Core.Entities.OrderAggregate;
using System;
using System.Linq;

namespace Core.Entities.OrderNeo4j
{
    public class ProductNeo4j 
    {
        public ProductNeo4j()
        {

        }
        public ProductNeo4j(Product product)
        {
            uuid = Guid.NewGuid().ToString();
            ProductItemId = product._id;
            ProductName = product.Name;
            PictureUrl = product._photos.Where(e => e.IsMain).Select(e => e.PictureUrl).SingleOrDefault();
            Price = product.Price;
        }
        public string uuid { get; set; }
        public string ProductItemId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
        public decimal Price { get; set; }
        //public bool isAvailable { get; set; }
    }
}
