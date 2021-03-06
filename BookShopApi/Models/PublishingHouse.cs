﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class PublishingHouse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [BsonIgnoreIfNull]
        public DateTime? UpdateAt { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? DeleteAt { get; set; }
    }
}
