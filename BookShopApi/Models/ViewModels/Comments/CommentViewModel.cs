using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Models.ViewModels.Comments
{
    public class CommentViewModel
    {
       
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  
        public string UserFullName { get; set; } 
        public string Title { get; set; }
        public int Rate { get; set; }
        public string Content { get; set; }
        public DateTime CreateAt { get; set; } 
       
    }
}
