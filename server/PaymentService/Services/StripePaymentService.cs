using AutoMapper;
using Contract;
using MassTransit;
using MediatR;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using PaymentService.Services.Payment;
using Stripe;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class StripePaymentService : IWebhookPaymentService
    {
        private readonly IPaymentUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMediator _mediator;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            IPaymentUnitOfWork unitOfWork,
            IConfiguration config,
            IMapper mapper,
            IPublishEndpoint publishEndpoint,
            IMediator mediator,
            ILogger<StripePaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default)
        {
            var webhookSecret = _config["StripeSettings:WebhookSecret"];

            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signatureHeader,
                webhookSecret);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var orderId = paymentIntent?.Metadata["orderId"]
                        ?? throw new InvalidOperationException("OrderId not found in metadata");

                    _logger.LogInformation("Payment succeeded for OrderId: {OrderId}", orderId);

                    await _publishEndpoint.Publish(new PaymentCompleted
                    {
                        OrderId = orderId
                    }, cancellationToken);

                    await _mediator.Send(new UpdatePaymentStatusCommand
                    {
                        OrderId = orderId,
                        NewStatus = PaymentStatus.Succeeded
                    }, cancellationToken);

                    break;
                }

                case "payment_intent.payment_failed":
                {
                    var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                    var orderId = failedIntent?.Metadata["orderId"]
                        ?? throw new InvalidOperationException("OrderId not found in metadata");

                    var errorMessage = failedIntent?.LastPaymentError?.Message ?? "Payment failed";

                    _logger.LogWarning("Payment failed for OrderId: {OrderId}, Reason: {Reason}", orderId, errorMessage);

                    await _publishEndpoint.Publish(new PaymentFailed
                    {
                        OrderId = orderId,
                        ErrorMessage = errorMessage
                    }, cancellationToken);

                    await _mediator.Send(new UpdatePaymentStatusCommand
                    {
                        OrderId = orderId,
                        NewStatus = PaymentStatus.Failed
                    }, cancellationToken);

                    break;
                }

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(string userId, string orderId, decimal amount)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var service = new PaymentIntentService();

            if (amount <= 0)
            {
                throw new InvalidOperationException($"Failed to create payment. Subtotal: {amount}, Order ID: {orderId}");
            }

            var options = new PaymentIntentCreateOptions
            {
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Amount = (long)amount,
                Currency = "vnd",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "orderId", orderId }
                }
            };

            var intent = await service.CreateAsync(options);

            return new PaymentIntentDto
            {
                Id = intent.Id,
                ClientSecret = intent.ClientSecret
            };
        }

        public async Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            var intent = await CreatePaymentIntentAsync(createPaymentDto.UserId, createPaymentDto.OrderId, createPaymentDto.Amount);

            var payment = Entities.Payment.CreatePayment(
                userId: createPaymentDto.UserId,
                orderId: createPaymentDto.OrderId,
                amount: createPaymentDto.Amount,
                paymentMethod: Enum.Parse<PaymentMethodType>(createPaymentDto.PaymentMethod)
            );

            payment.PaymentIntentId = intent.Id;
            payment.ClientSecret = intent.ClientSecret;

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}