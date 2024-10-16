using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoteachApi.Models
{
    public class Log
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement()]
        public string? CustomerId { get; set; }
        [BsonElement()]
        public string? CustomerEmail { get; set; }
        [BsonElement()]
        public long AmountTotal { get; set; }
        [BsonElement()]
        public string? PaymentIntentId { get; set; }
        [BsonElement()]
        public string? GoogleId { get; set; }
        [BsonElement()]
        public bool IsActivated { get; set; } = false;
        [BsonElement()]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
