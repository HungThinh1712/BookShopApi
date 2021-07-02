using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models
{
    public class Promotion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
        public List<string> CustomerIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CountApply { get; set; }
        public string Description { get; set; }
        public PromotionType PromotionType { get; set; }
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MinMoney { get; set; }
        [BsonRepresentation(BsonType.Decimal128)]

        public decimal DiscountMoney { get; set; } = 0;
        public List<string> BookIds { get; set; }
        public List<string> CustomerApplied { get; set; } = new List<string>();
        public PromotionStatus Status { get; set; } = PromotionStatus.InActive;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public enum PromotionType: byte
    {
        Discount,
        FreeShip
    }
    public enum PromotionStatus : byte
    {
        InActive,
        Active,
        OnGoing,
        Canceled,
        Expired
    }
}
