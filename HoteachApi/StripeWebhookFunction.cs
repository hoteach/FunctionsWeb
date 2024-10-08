using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SendGrid;
using SendGrid.Helpers.Mail;
using Stripe;
using Stripe.Checkout;

namespace HoteachApi
{
    public class StripeWebhookFunction()
    {
        private static readonly MongoClient _mongoClient = new (Environment.GetEnvironmentVariable("MongoDBConnectionString"));
        private static readonly IMongoDatabase _database = _mongoClient.GetDatabase("hoteach-v1");

        [Function("StripeWebhook")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ParseEvent(json);
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;

                var collection = _database.GetCollection<BsonDocument>("users");
                var document = new BsonDocument
                {
                    { "CustomerId", session.CustomerId },
                    { "CustomerEmail", session.CustomerEmail },
                    { "AmountTotal", session.AmountTotal },
                    { "PaymentIntentId", session.PaymentIntentId },
                    { "PaymentIntentId", session.PaymentIntentId }
                };
                await collection.InsertOneAsync(document);

                var client = new SendGridClient(Environment.GetEnvironmentVariable("SendGridApiKey"));
                var msg = new SendGridMessage
                {
                    From = new EmailAddress("no-reply@hoteach.com", "HoTeach"),
                    Subject = "Account Activation",
                    HtmlContent = $"<p>Thank you for your purchase! <a href='https://hoteach.com/activate?id={session.PaymentIntentId}'>Click here to activate your account.</a></p>"
                };
                msg.AddTo(new EmailAddress(session.CustomerEmail));
                await client.SendEmailAsync(msg);

                return new OkObjectResult($"Processed: {stripeEvent.Id}");
            }

            return new BadRequestResult();
        }
    }
}
