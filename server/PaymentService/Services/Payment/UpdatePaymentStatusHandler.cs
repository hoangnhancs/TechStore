using MediatR;
using PaymentService.Persistence;
using Shared.Core.EF.Application;

namespace PaymentService.Services.Payment
{
    public class UpdatePaymentStatusHandler : IRequestHandler<UpdatePaymentStatusCommand, AppResult<Unit>>
    {
        private readonly IPaymentUnitOfWork _unitOfWork;

        public UpdatePaymentStatusHandler(IPaymentUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AppResult<Unit>> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (payment == null)
                return AppResult<Unit>.Failure("Payment not found", 404);

            payment.Status = request.NewStatus;
            await _unitOfWork.CommitAsync(cancellationToken);

            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}
