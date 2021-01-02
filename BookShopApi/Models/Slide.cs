using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookShopApi.Models
{
    public class Slide
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public bool IsDeleted { get; set; }
    }
}
