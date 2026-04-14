using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using Stripe;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class StripePaymentService : IWebhookPaymentService
    {
        private IPaymentUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public StripePaymentService(IPaymentUnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _mapper = mapper;
        }
        public async Task<PaymentDto> HandleWebhook(HttpRequest request)
        {
            throw new NotImplementedException();
        }
        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(string userId, string orderId, decimal amount)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];

            var service = new PaymentIntentService();
            var intent = new PaymentIntent();

       
            if (amount <= 0)
            {
                Console.WriteLine($"Error: Subtotal negative than zero for order ID: {orderId}, Subtotal: {amount}");
                throw new InvalidOperationException($"Failed to create payment. Subtotal: {amount}, Order ID: {orderId}");
            }           
        
            
        
            var options = new PaymentIntentCreateOptions
            {
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Amount = (long)amount, // Convert to cents
                Currency = "vnd",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "orderId", orderId }
                }
            };

            intent = await service.CreateAsync(options);

            return new PaymentIntentDto() { Id = intent.Id, ClientSecret = intent.ClientSecret } ?? throw new InvalidOperationException("PaymentIntent creation failed.");

        }

        public async Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            //tạo payment intent trước để lấy client secret, sau đó mới trả về thông tin payment
            var intent = await CreatePaymentIntentAsync(createPaymentDto.UserId, createPaymentDto.OrderId, createPaymentDto.Amount);
            var payment = Entities.Payment.CreatePayment(
                userId: createPaymentDto.UserId,
                orderId: createPaymentDto.OrderId,
                amount: createPaymentDto.Amount,
                paymentMethod: Enum.Parse<PaymentMethodType>(createPaymentDto.PaymentMethod.ToString())
            );
            payment.PaymentIntentId = intent.Id;
            payment.ClientSecret = intent.ClientSecret;
            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.CommitAsync();
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}