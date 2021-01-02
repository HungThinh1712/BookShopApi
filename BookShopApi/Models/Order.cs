using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShopApi.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public string OrderId { get; set; }
        public  List<ItemInCart> Items { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public int PaymentId { get; set; }

       
    }
}
