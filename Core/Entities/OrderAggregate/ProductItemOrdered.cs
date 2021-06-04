namespace Core.Entities.OrderAggregate
{
    public class ProductItemOrdered
    {
        public ProductItemOrdered()
        {
        }

        public ProductItemOrdered(string productItemId, string productName, string pictureUrl)
        {
            this.ProductItemId = productItemId;
            this.ProductName = productName;
            this.PictureUrl = pictureUrl;

        }
        public string ProductItemId { get; set; }
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
    }
}