using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order
{
    public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, AppResult<Unit>>
    {
        private readonly IOrderUnitOfWork _unitOfWork;
        public UpdateOrderStatusHandler(IOrderUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return AppResult<Unit>.Failure("Order not found", 404);
            }
            try
            {
                order.UpdateStatus(request.NewStatus);
            }
            catch (InvalidOperationException ex)
            {
                return AppResult<Unit>.Failure(ex.Message, 400);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}