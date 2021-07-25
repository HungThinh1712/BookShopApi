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
        public string BirthDay { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
