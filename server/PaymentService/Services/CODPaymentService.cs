using AutoMapper;
using PaymentService.DTOs;
using PaymentService.Persistence;
using PaymentService.Services.Interface;
using static PaymentService.Entities.Payment;

namespace PaymentService.Services
{
    public class CODPaymentService : IPaymentService
    {
        private readonly IPaymentUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CODPaymentService(IPaymentUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
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
                Status = PaymentStatus.Pending
            };
            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.CommitAsync();
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}