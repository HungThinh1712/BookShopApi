using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShopApi.Models
{
    public class Comment
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
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
    public class CommentWithUserFullName : Comment
    {
        public User[] User { get; set; }

    }
}
