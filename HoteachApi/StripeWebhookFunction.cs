using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using SendGrid;
using SendGrid.Helpers.Mail;
using Stripe;
using Stripe.Checkout;

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
                From = new EmailAddress("grishopompata@gmail.com", "HoTeach"),
                Subject = "Activate Your HoTeach Account",
                HtmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f7f7f7;'>
                        <div style='max-width: 600px; margin: auto; background-color: white; border-radius: 15px; padding: 40px; text-align: center;'>
                            <div style='background-color: #ff3300; width: 80px; height: 80px; border-radius: 50%; margin: auto; display: flex; align-items: center; justify-content: center;'>
                                <img src='https://i.imgur.com/zCDfngd.png' alt='Success' style='width:100%;' />
                            </div>
                            <h2 style='color: #333333; font-size: 28px; margin: 20px 0;'>Thank you for your purchase!</h2>
                            <p style='color: #777777; font-size: 16px; margin-bottom: 20px;'>You're one step away from unlocking your full potential.</p>
                            <a href='http://localhost:5173/activate?id={session.PaymentIntentId}' style='background-color: #ffe505; color: #000; border: 2px solid black; text-decoration: none; padding: 15px 30px; border-radius: 10px; font-size: 16px; font-weight: bold; display: inline-block;'>Activate Your Account</a>
    
                            <div style='margin-top: 40px; display: flex; justify-content: center; gap: 30px;'>
                                <div style='text-align: center;'>
                                    <img src='https://i.imgur.com/A6fCIRY.png' alt='Instant Access' style='width: 40px; height: 40px;' />
                                    <p style='color: #333333; font-weight: bold; margin-top: 10px;'>Instant Access</p>
                                </div>
                                <div style='text-align: center;'>
                                    <img src='https://i.imgur.com/xCgK3qC.png' alt='Exclusive Content' style='width: 40px; height: 40px;' />
                                    <p style='color: #333333; font-weight: bold; margin-top: 10px;'>Exclusive Content</p>
                                </div>
                            </div>
    
                            <div style='margin-top: 30px;'>
                                <p style='color: #777777;'>Your journey to success starts now!</p>
                                <p style='color: #777777;'>Get ready to transform your career!</p>
                            </div>
                        </div>
                    </body>
                </html>"
            };
            msg.AddTo(new EmailAddress(email));

            await client.SendEmailAsync(msg);
        }

    }
}
