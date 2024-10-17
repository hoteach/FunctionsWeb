using HoteachApi.Contracts.Helpers;
using HoteachApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace HoteachApi
{
    public class GeneratePath(ChatClient client, IMongoClient mongoClient)
    {
        [Function("GeneratePath")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty.");
            }

            var data = JsonConvert.DeserializeObject<GoogleIdRequest>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.GoogleId))
                return new BadRequestObjectResult("GoogleId is required.");

            var database = mongoClient.GetDatabase("hoteach-v1");
            var preferences = database.GetCollection<UserPreferences>("preferences");

            var userPreferences = await preferences.Find(u => u.GoogleId == data.GoogleId).FirstOrDefaultAsync();

            if(userPreferences == null)
                return new BadRequestObjectResult("GoogleId is required.");

            var path = database.GetCollection<Models.Path>("path-generations");

            var systemMessage = GeneratePrompt.SystemMessage();
            var userMessage = GeneratePrompt.UserMessage(userPreferences);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(userMessage)
            };

            var response = await client.CompleteChatAsync(messages);
            var responceText = response.Value.Content[0].Text;

            await path.InsertOneAsync(new Models.Path
            {
                Request = userMessage,
                PathContent = responceText
            });

            return new OkObjectResult(responceText);
        }
    }
}
