using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using HoteachApi.Models;
using MongoDB.Bson;

namespace HoteachApi
{
    public class GetUserData
    {
        private readonly IMongoClient _mongoClient;

        public GetUserData(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        [Function("GetUserData")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string googleId = data?.googleId;

            var database = _mongoClient.GetDatabase("hoteach-v1");
            var collection = database.GetCollection<User>("users");

            var user = await collection.Find(u => u.GoogleId == googleId).FirstOrDefaultAsync();

            if (user != null)
            {
                return new OkObjectResult(user.ToJson());
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
    }
}
