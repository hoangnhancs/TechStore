using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PaymentService.DTOs;
using PaymentService.Services.Interface;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services.Payment
{
    public class CODPaymentService : IPaymentService
    {
        private readonly IMapper _mapper;
        public CODPaymentService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task<PaymentDto> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            var payment = new Entities.Payment
            {
                UserId = createPaymentDto.UserId,
                OrderId = createPaymentDto.OrderId,
                Amount = createPaymentDto.Amount,
                PaymentMethod = Enum.Parse<PaymentMethodType>(createPaymentDto.PaymentMethod.ToString()),
                Status = PaymentStatus.Processing
            };
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}