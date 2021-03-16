using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Authors
{
    public class AuthorsInAdminViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CreateAt { get; set; }
        [BsonIgnoreIfNull]
        public string UpdateAt { get; set; }
        [BsonIgnoreIfNull]
        public string DeleteAt { get; set; }
        public int SumBooks { get; set; }
    }
}
