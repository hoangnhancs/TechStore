using AutoMapper;
using MediatR;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.OrderHistory
{
    public class GetOrderStatusHistoryHandler : IRequestHandler<GetOrderStatusHistoryQuery, AppResult<OrderStatusHistoryWithShipmentDto>>
    {
        private readonly IOrderUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderStatusHistoryHandler(IOrderUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<OrderStatusHistoryWithShipmentDto>> Handle(GetOrderStatusHistoryQuery request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdWithHistoriesAndShipmentAsync(request.OrderId, cancellationToken);

            if (order == null)
                return AppResult<OrderStatusHistoryWithShipmentDto>.Failure("Order not found", 404);

            if (order.UserId != request.UserId)
                return AppResult<OrderStatusHistoryWithShipmentDto>.Failure("Unauthorized access to order history", 403);

            return AppResult<OrderStatusHistoryWithShipmentDto>.Success(new OrderStatusHistoryWithShipmentDto
            {
                StatusHistory = order.StatusHistories == null
                    ? []
                    : _mapper.Map<List<OrderStatusHistoryDto>>(order.StatusHistories),
                Shipment = order.Shipment == null ? null : _mapper.Map<ShipmentDto>(order.Shipment)
            });
        }
    }
}
