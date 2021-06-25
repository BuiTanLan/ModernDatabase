using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entities
{
    public class Product : BaseEntityMongo
    {
        public Product()
        {
            Photos = new List<Photo>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        //public string PictureUrl { get; set; }
        public ProductType ProductType { get; set; }
        //public int ProductTypeId { get; set; }
        public ProductBrand ProductBrand { get; set; }
        //public int ProductBrandId { get; set; }
        private readonly List<Photo> _photos;
        public List<Photo> Photos { get; set; }

        public void AddPhoto(string pictureUrl, string fileName, bool isMain = false)
        {
            var photo = new Photo
            {
                FileName = fileName,
                PictureUrl = pictureUrl
            };

            if(Photos.Count == 0) photo.IsMain = true;

            if (Photos == null)
                Photos = new List<Photo>();

            Photos.Add(photo);
        }

        public void RemovePhoto(int id)
        { 
            var photo = Photos.Find(x => x.Id == id);
            Photos.Remove(photo);
        }

        public void SetMainPhoto(int id)
        {
            var currentMain = Photos.SingleOrDefault(x => x.IsMain);
            foreach (var item in Photos.Where(item => item.IsMain))
            { 
                item.IsMain = false;
            }

            var photo = Photos.Find(x => x.Id == id);
            if (photo != null)
            {
                photo.IsMain = true;
                if (currentMain != null) currentMain.IsMain = false;
            }

        }
    }
}