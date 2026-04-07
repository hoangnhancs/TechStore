using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Controller;
using Stripe;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseApiController
    {
        [HttpPost("webhook/stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            // var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            
            // // Verify signature để chắc chắn request từ Stripe
            // var stripeEvent = EventUtility.ConstructEvent(
            //     json,
            //     Request.Headers["Stripe-Signature"],
            //     _stripeSettings.WebhookSecret // whsec_xxx từ dashboard hoặc CLI
            // );

            // switch (stripeEvent.Type)
            // {
            //     case Events.PaymentIntentSucceeded:
            //         var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            //         await _bus.Publish(new PaymentCompleted
            //         {
            //             OrderId = paymentIntent.Metadata["OrderId"]
            //         });
            //         break;

            //     case Events.PaymentIntentPaymentFailed:
            //         await _bus.Publish(new PaymentFailed
            //         {
            //             OrderId = paymentIntent.Metadata["OrderId"],
            //             ErrorMessage = paymentIntent.LastPaymentError?.Message
            //         });
            //         break;
            // }

            return Ok();
        }
    }
}