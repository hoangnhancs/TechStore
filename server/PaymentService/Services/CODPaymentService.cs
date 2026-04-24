using AutoMapper;
using Contract;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using PaymentService.SignalR;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class CODPaymentService : IPaymentService
    {
        private readonly IPaymentUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly IHubContext<PaymentHub> _hubContext;
        public CODPaymentService(IPaymentUnitOfWork unitOfWork, 
        IPublishEndpoint publishEndpoint, 
        IMapper mapper,
        IHubContext<PaymentHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        public async Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            var payment = new Entities.Payment
            {
                UserId = createPaymentDto.UserId,
                OrderId = createPaymentDto.OrderId,
                Amount = createPaymentDto.Amount,
                PaymentMethod = Enum.Parse<PaymentMethodType>(createPaymentDto.PaymentMethod.ToString()),
                Status = PaymentStatus.Pending
            };
            await _unitOfWork.PaymentRepository.AddAsync(payment);
            //await _publishEndpoint.Publish(new
            //{
            //    UserId = payment.UserId,
            //    OrderId = payment.OrderId,
            //    Amount = payment.Amount,
            //    PaymentMethod = payment.PaymentMethod.ToString(),
            //    Status = payment.Status.ToString()
            //}, context =>
            //{
            //    context.SetRoutingKey("payment.created");
            //});
            await _publishEndpoint.Publish(new PaymentCompleted
            {
                OrderId = createPaymentDto.OrderId
            });
            await _unitOfWork.CommitAsync();
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            await _hubContext.Clients
                    .Group(paymentDto.OrderId)
                    .SendAsync("ReceivePayment", paymentDto);

            return paymentDto;
        }
    }
}