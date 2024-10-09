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
        private readonly string _stripeApiKey;

        public StripeWebhookFunction(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _stripeApiKey = Environment.GetEnvironmentVariable("StripeSecretKey");
            StripeConfiguration.ApiKey = _stripeApiKey;
        }

        [Function("StripeWebhook")]
        public async Task<HttpResponseData> Run(
               [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            string json = await new StreamReader(req.Body).ReadToEndAsync();

            var database = _mongoClient.GetDatabase("hoteach-v1");
            var collection = database.GetCollection<BsonDocument>("users");

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
            }
            catch (StripeException)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid Stripe event.");
                return badRequestResponse;
            }

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                if (stripeEvent.Data?.Object is Session session)
                {
                    try
                    {
                        var customerOptions = new CustomerCreateOptions
                        {
                            Email = session.CustomerDetails.Email,
                        };

                        var customerService = new CustomerService();
                        var customer = await customerService.CreateAsync(customerOptions);

                        var document = new BsonDocument
                        {
                            { "CustomerId", customer.Id },
                            { "CustomerEmail", customer.Email },
                            { "AmountTotal", session.AmountTotal },
                            { "PaymentIntentId", session.PaymentIntentId }
                        };

                        await Task.WhenAll(
                            collection.InsertOneAsync(document),
                            SendConfirmationEmail(session, customer.Email)
                        );

                        var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                        await successResponse.WriteStringAsync($"Processed: {stripeEvent.Id}");
                        return successResponse;
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                        await errorResponse.WriteStringAsync("Internal Server Error.");
                        return errorResponse;
                    }
                }
            }

            var unsupportedResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await unsupportedResponse.WriteStringAsync("Unsupported event type.");
            return unsupportedResponse;
        }

        private static async Task SendConfirmationEmail(Session session, string email)
        {
            var client = new SendGridClient(Environment.GetEnvironmentVariable("SendGridApiKey"));
            var msg = new SendGridMessage
            {
                From = new EmailAddress("no-reply@hoteach.com", "HoTeach"),
                Subject = "Account Activation",
                HtmlContent = $"<p>Thank you for your purchase! <a href='https://hoteach.com/activate?id={session.PaymentIntentId}'>Click here to activate your account.</a></p>"
            };
            msg.AddTo(new EmailAddress(email));

            await client.SendEmailAsync(msg);
        }
    }
}
