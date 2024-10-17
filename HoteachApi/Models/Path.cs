using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HoteachApi.Models
{
    public class Path
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement()]
        public string? Request { get; set; }
        [BsonElement()]
        public string? PathContent { get; set; }
    }
}
