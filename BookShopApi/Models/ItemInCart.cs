using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class ItemInCart
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal CoverPrice { get; set; }
        public string AuthorName { get; set; }
        public string ImageSrc { get; set; }
    }
}
