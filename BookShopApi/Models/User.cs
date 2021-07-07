using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace BookShopApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string PassWord { get; set; }
        public bool IsActive { get; set; }
        public string CodeActive { get; set; }
        public string CodeResetPassWord { get; set; }

        public string SpecificAddress { get; set; }
        //public string ProvinceId { get; set; }
        //public string ProvinceName { get; set; }
        //public string DistrictId { get; set; }
        //public string DistrictName { get; set; }
        //public string WardId { get; set; }
        //public string WardName { get; set; }
        public int Sex { get; set; }

        public string ImgUrl { get; set; }

        public DateTime BirthDay { get; set; }
        [BsonIgnoreIfNull]
        public string? Distance { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

   
        public DateTime? UpdateAt { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? DeleteAt { get; set; }
        public bool IsAdmin { get; set; } = false;
        [BsonIgnoreIfNull]
        public bool IsTopAdmin { get; set; } = false;
    }
}
