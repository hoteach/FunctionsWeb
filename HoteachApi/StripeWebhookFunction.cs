using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SendGrid;
using SendGrid.Helpers.Mail;
using Stripe;
using Stripe.Checkout;
using System.IO;
using System.Threading.Tasks;
using System;

namespace HoteachApi
{
    public class StripeWebhookFunction
    {
        private readonly IMongoClient _mongoClient;

        public StripeWebhookFunction(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        [Function("StripeWebhook")]
        public async Task<HttpResponseData> Run(
               [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
               FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("StripeWebhook");

            // Read the request body
            string json = await new StreamReader(req.Body).ReadToEndAsync();

            // Parse the event from Stripe
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Invalid Stripe event");
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid Stripe event.");
                return badRequestResponse;
            }

            // Handle checkout session completed
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                if (stripeEvent.Data?.Object is Session session)
                {
                    var database = _mongoClient.GetDatabase("hoteach-v1");
                    var collection = database.GetCollection<BsonDocument>("users");
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
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing Stripe event");
                        var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                        await errorResponse.WriteStringAsync("Internal Server Error.");
                        return errorResponse;
                    }

                    var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await successResponse.WriteStringAsync($"Processed: {stripeEvent.Id}");
                    return successResponse;
                }
                else
                {
                    logger.LogWarning("Stripe event object is not a Session");
                }
            }

            var unsupportedResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await unsupportedResponse.WriteStringAsync("Unsupported event type.");
            return unsupportedResponse;
        }

        private async Task SendConfirmationEmail(Session session)
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
