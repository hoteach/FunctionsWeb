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
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string? googleId = data?.googleId;
            string activationId = data?.paymentIntentId;

            var database = _mongoClient.GetDatabase("hoteach-v1");
            var collection = database.GetCollection<User>("users");
            var collection2 = database.GetCollection<User>("logs");

            var user = await collection.Find(u => u.PaymentIntentId == activationId).FirstOrDefaultAsync();

            var user2 = user;
            user2.GoogleId = googleId;

            await collection2.InsertOneAsync(user2);

            if (user != null)
            {
                var updateDefinition = Builders<User>.Update
                    .Set(u => u.GoogleId, googleId)
                    .Set(u => u.IsActivated, true)
                    .Unset(u => u.PaymentIntentId);

                await collection.UpdateOneAsync(u => u.Id == user.Id, updateDefinition);

                return new OkObjectResult("Account activated successfully.");
            }
            else
            {
                return new OkObjectResult("No activation.");
            }
        }
    }
}
