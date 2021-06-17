﻿using System;
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
        public List<ItemInCart> Items { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalMoney { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; }
        public ConfirmStatus ConfirmStatus { get; set; } = Models.ConfirmStatus.None;
        public int PaymentType { get; set; }
        public string CancelReason { get; set; } = null;

       
    }
    public class OrderWithUserName : Order
    {
        public User[] User { get; set; }
    }
    public enum OrderStatus : byte
    {
        DangChoXacNhan,
        DaXacNhan,
        DangGiaoHang,
        DaGiaoHang,
        Huy
    }
    public enum ConfirmStatus : byte
    {
        None,
        Seller,
        Buyer,
        Both
    }
}
