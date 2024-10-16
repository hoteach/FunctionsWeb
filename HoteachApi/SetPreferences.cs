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
    public class SetPreferences(IMongoClient mongoClient)
    {
        private readonly IMongoClient _mongoClient = mongoClient;

        [Function("SetPreferences")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var preferences = JsonConvert.DeserializeObject<UserPreferences>(requestBody)
                ?? throw new ArgumentNullException(nameof(req));

            var database = _mongoClient.GetDatabase("hoteach-v1");
            var users = database.GetCollection<User>("users");
            var userPreferences = database.GetCollection<UserPreferences>("preferences");

            var user = await users.Find(u => u.GoogleId == preferences.GoogleId).FirstOrDefaultAsync();

            if (user == null || user.HasPreferences)
            {
                throw new ArgumentNullException(nameof(req));
            }
            else
            {
                await userPreferences.InsertOneAsync(preferences);

                var updateDefinition = Builders<User>.Update
                    .Set(u => u.HasPreferences, true);
                await users.UpdateOneAsync(u => u.Id == user.Id, updateDefinition);

                return new OkObjectResult(user.ToJson());
            }
        }
    }
}
