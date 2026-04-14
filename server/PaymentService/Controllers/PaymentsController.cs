using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Controller;
using Stripe;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseApiController
    {
        private readonly IConfiguration _config;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IConfiguration config, 
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentsController> logger)
        {
            _config = config;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpPost("~/webhook/stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var whs = _config["StripeSettings:WebhookSecret"];
                // Verify signature to ensure request is from Stripe
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    whs // whsec_xxx from dashboard or CLI
                );

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogInformation("Payment succeeded for OrderId: {OrderId}", paymentIntent?.Metadata["orderId"]);

                        await _publishEndpoint.Publish(new PaymentCompleted
                        {
                            OrderId = paymentIntent?.Metadata["orderId"] ?? throw new InvalidOperationException("OrderId not found in metadata")
                        });
                        break;

                    case "payment_intent.payment_failed":
                        var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogWarning("Payment failed for OrderId: {OrderId}, Reason: {Reason}", 
                            failedIntent?.Metadata["orderId"], 
                            failedIntent?.LastPaymentError?.Message);

                        await _publishEndpoint.Publish(new PaymentFailed
                        {
                            OrderId = failedIntent?.Metadata["orderId"] ?? throw new InvalidOperationException("OrderId not found in metadata"),
                            ErrorMessage = failedIntent?.LastPaymentError?.Message ?? "Payment failed"
                        });
                        break;

                    default:
                        _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return BadRequest();
            }
        }
    }
}