using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using HoteachApi.Models;

namespace HoteachApi
{
    public class ActivateUser
    {
        private readonly IMongoClient _mongoClient;

        public ActivateUser(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        [Function("ActivateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty.");
            }

            var data = JsonConvert.DeserializeObject<ActivationRequest>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.PaymentIntentId))
            {
                return new BadRequestObjectResult("PaymentIntentId is required.");
            }

            var database = _mongoClient.GetDatabase("hoteach-v1");
            var collection = database.GetCollection<User>("users");
            var logs = database.GetCollection<Log>("logs");

            var user = await collection.Find(u => u.PaymentIntentId == data.PaymentIntentId).FirstOrDefaultAsync();

            if (user != null)
            {
                var log = new Log()
                {
                    GoogleId = data.GoogleId,
                    AmountTotal = user.AmountTotal,
                    PaymentIntentId = data.PaymentIntentId,
                    IsActivated = user.IsActivated
                };

                var updateDefinition = Builders<User>.Update
                    .Set(u => u.GoogleId, data.GoogleId)
                    .Set(u => u.IsActivated, true)
                    .Unset(u => u.PaymentIntentId);

                await collection.UpdateOneAsync(u => u.Id == user.Id, updateDefinition);
                await logs.InsertOneAsync(log);

                return new OkObjectResult("Account activated successfully.");
            }
            else
            {
                return new OkObjectResult("No activation.");
            }
        }
    }
}
