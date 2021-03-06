﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public string SenderId { get; set; }
        public string TimeAgo { get; set; }
        [BsonIgnore]
        public int TotalRead { get; set; }
        [BsonIgnore]
        public string  ImgUrl { get; set; }
        public int Status { get; set; }
        public string OrderId { get; set; }
        public string OrderCode { get; set; }

    }
}
