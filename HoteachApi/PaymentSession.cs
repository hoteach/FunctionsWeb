using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace HoteachApi
{
    public class PaymentSession
    {
        [Function("PaymentSession")]
        public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var stripeSecretKey = Environment.GetEnvironmentVariable("StripeSecretKey");
            StripeConfiguration.ApiKey = stripeSecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = "price_1Q7hduDfNdyg9lxWbJOsRkh3",
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "https://your-site.com/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://your-site.com/cancel" // Optional: Add a Cancel URL
            };

            var service = new SessionService();
            var session = service.Create(options);

            return new OkObjectResult(new { id = session.Id });
        }
    }
}
