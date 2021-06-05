using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShopApi.Models.ViewModels
{
    public class BooksViewModel
    {
        public string Id { get; set; }
        public string BookName { get; set; }

        public string Price { get; set; }

        public string CoverPrice { get; set; }

        public string ImgUrl { get; set; }
        public double Rating { get; set; }
        public int CountRating { get; set; }
        public string TypeId { get; set; }
    }
}
