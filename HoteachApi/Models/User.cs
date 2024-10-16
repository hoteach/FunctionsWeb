﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoteachApi.Models
{
    public class User
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
        public bool HasPreferences { get; set; } = false;
        [BsonElement()]
        public bool HasLearningPath { get; set; } = false;
    }
}
