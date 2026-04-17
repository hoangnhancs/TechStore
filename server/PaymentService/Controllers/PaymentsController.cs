using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;
using PaymentService.Services.Interface;
using Shared.Web.Controller;
using Stripe;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseApiController
    {
        private readonly StripePaymentService _stripePaymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            StripePaymentService stripePaymentService,
            ILogger<PaymentsController> logger)
        {
            _stripePaymentService = stripePaymentService;
            _logger = logger;
        }

        [HttpPost("~/webhook/stripe")]
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
        {
            var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
            var signature = Request.Headers["Stripe-Signature"].ToString();

            try
            {
                await _stripePaymentService.HandleWebhookAsync(payload, signature, cancellationToken);
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