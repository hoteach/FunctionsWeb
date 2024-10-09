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
            string googleId = data?.googleId;
            string activationId = data?.paymentIntentId;

            var database = _mongoClient.GetDatabase("hoteach-v1");
            var collection = database.GetCollection<User>("users");

            var user = await collection.Find(u => u.PaymentIntentId == activationId).FirstOrDefaultAsync();

            if (user != null)
            {
                user.GoogleId = googleId;
                user.IsActivated = true;
                user.PaymentIntentId = null;
                await collection.ReplaceOneAsync(u => u.CustomerId == user.CustomerId, user);
                return new OkObjectResult("Account activated successfully.");
            }
            else
            {
                var newUser = new User
                {
                    GoogleId = googleId,
                    IsActivated = false,
                    PaymentIntentId = null
                };
                await collection.InsertOneAsync(newUser);
                return new OkObjectResult("User created without activation.");
            }
        }
    }
}
