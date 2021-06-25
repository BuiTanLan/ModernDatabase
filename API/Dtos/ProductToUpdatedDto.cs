
using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class ProductToUpdatedDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string ProductTypeId { get; set; }
        [Required]  
        public string ProductBrandId { get; set; }
    }
}