using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Comments
{
    public class CommentUpdateModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; }

        public string Title { get; set; }

        public int Rate { get; set; }

        public string Content { get; set; }
        public string OrderId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
