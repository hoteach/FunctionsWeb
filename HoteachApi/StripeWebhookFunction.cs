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
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace HoteachApi
{
    public class StripeWebhookFunction
    {
        private static readonly MongoClient _mongoClient = new(Environment.GetEnvironmentVariable("MongoDBConnectionString"));
        private static readonly IMongoDatabase _database = _mongoClient.GetDatabase("hoteach-v1");
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2) // Set timeout limit for long-running tasks
        };

        [Function("StripeWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing Stripe webhook");

            // Read the request body
            string json = await new StreamReader(req.Body).ReadToEndAsync();

            // Parse the event from Stripe
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ParseEvent(json);
            }
            catch (StripeException ex)
            {
                log.LogError($"Failed to parse Stripe event: {ex.Message}");
                return new BadRequestObjectResult("Invalid Stripe event.");
            }

            // Handle checkout session completed
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;

                if (session != null)
                {
                    // MongoDB document creation
                    var collection = _database.GetCollection<BsonDocument>("users");
                    var document = new BsonDocument
                    {
                        { "CustomerId", session.CustomerId },
                        { "CustomerEmail", session.CustomerEmail },
                        { "AmountTotal", session.AmountTotal },
                        { "PaymentIntentId", session.PaymentIntentId }
                    };

                    try
                    {
                        // Insert document into MongoDB and send email concurrently
                        await Task.WhenAll(
                            collection.InsertOneAsync(document),
                            SendConfirmationEmail(session)
                        );
                        log.LogInformation($"Successfully processed session for Customer: {session.CustomerId}");
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Error processing Stripe session: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                    return new OkObjectResult($"Processed: {stripeEvent.Id}");
                }
            }

            log.LogWarning("Received an unsupported event type");
            return new BadRequestResult();
        }

        private static async Task SendConfirmationEmail(Session session)
        {
            var client = new SendGridClient(Environment.GetEnvironmentVariable("SendGridApiKey"));
            var msg = new SendGridMessage
            {
                From = new EmailAddress("no-reply@hoteach.com", "HoTeach"),
                Subject = "Account Activation",
                HtmlContent = $"<p>Thank you for your purchase! <a href='https://hoteach.com/activate?id={session.PaymentIntentId}'>Click here to activate your account.</a></p>"
            };
            msg.AddTo(new EmailAddress(session.CustomerEmail));

            await client.SendEmailAsync(msg);
        }
    }
}
