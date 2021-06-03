using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BookName { get; set; }

        public string Description { get; set; }

        public DateTime PublishDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string PublishHouseId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuthorId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal CoverPrice { get; set; }
        public string ImgUrl { get; set; }

        public int Amount { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TagId { get; set; }

        public string ZoneType { get; set; }

        public string Alias { get; set; }

        public string Size { get; set; }

        public string PageAmount { get; set; }

        public string CoverType { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [BsonIgnoreIfNull]
        public DateTime ? UpdateAt { get; set; }

        [BsonIgnoreIfNull]
        public DateTime ? DeleteAt { get; set; } 

        public string Code { get; set; }

        [BsonIgnoreIfNull]
        public List<Comment> Comments { get; set; } = new List<Comment>();

    }
    
}
