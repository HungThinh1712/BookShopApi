using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Orders
{
    public class OrdersViewModel
    {
       
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }      
        public string OrderId { get; set; }
        public string Description { get; set; }
        public string TotalMoney { get; set; }
        public string CreateAt { get; set; }
        public List<Models.ViewModels.ItemInCartViewModel> Items { get; set; }
    }
}
