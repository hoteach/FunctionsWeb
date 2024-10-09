using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;

namespace HoteachApi
{
    public class PaymentSession
    {
        [Function("PaymentSession")]
        public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = "price_1Q7hduDfNdyg9lxWbJOsRkh3",
                        Quantity = 1,
                    },
                ],
                Mode = "payment",
                SuccessUrl = "https://your-site.com/success?session_id={CHECKOUT_SESSION_ID}"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return new OkObjectResult(new { id = session.Id });
        }
    }
}
