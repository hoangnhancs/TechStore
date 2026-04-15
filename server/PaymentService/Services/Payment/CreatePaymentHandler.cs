using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using Shared.Core.EF.Application;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services.Payment
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, AppResult<PaymentDto>>
    {
        private readonly IPaymentUnitOfWork _paymentUnitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
        public CreatePaymentHandler(IPaymentUnitOfWork paymentUnitOfWork, IMapper mapper, IPaymentServiceFactory paymentServiceFactory)
        {
            _paymentUnitOfWork = paymentUnitOfWork;
            _mapper = mapper;
            _paymentServiceFactory = paymentServiceFactory;
        }
        public async Task<AppResult<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
        //     var payment = new Entities.Payment
        //     {
        //         UserId = request.CreatePaymentDto.UserId,
        //         OrderId = request.CreatePaymentDto.OrderId,
        //         Amount = request.CreatePaymentDto.Amount,
        //         PaymentMethod = Enum.Parse<PaymentMethodType>(request.CreatePaymentDto.PaymentMethod),
        //     };
        //     try
        //     {
        //         var intent = await _paymentService.Create
        //     }
        //     catch (Exception ex)
        //     {
        //         return AppResult<PaymentDto>.Failure(ex.Message);
        //     }
            throw new NotImplementedException();   
        }
    }
}