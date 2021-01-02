using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShopApi.Models
{
    public class EnteredInvoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BookId { get; set; }

        public string Amount { get; set; }

        public DateTime Date { get; set; }
    }
}
