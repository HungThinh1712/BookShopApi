using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Books
{
    public class BookViewModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string BookName { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string CoverPrice { get; set; }
        public string ImgUrl { get; set; }
        public double Rating { get; set; }
        public int Amount { get; set; }
        public string BookTypeName { get; set; }
        public string AuthorName { get; set; }
        public string PublishingHouseName { get; set; }
        public string Size { get; set; }
   
        public string TagId { get; set; }
        public string TagName { get; set; }
        public string AuthorId { get; set; }
        public string PublishingHouseId { get; set; }

        public string PageAmount { get; set; }

        public string Cover_Type { get; set; }

        public string PublishDate { get; set; }
        public DateTime PublishDateAdmin { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }
        public string ZoneType { get; set; }
    }
}
