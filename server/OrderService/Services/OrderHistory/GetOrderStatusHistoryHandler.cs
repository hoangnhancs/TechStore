using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            var res = new OrderStatusHistoryWithShipmentDto();
            var order = (await _unitOfWork.OrderRepository.GetListAsync(
                p => p.Id == request.OrderId,
                q => q.Include(o => o.StatusHistories).Include(o => o.Shipment)
            )).FirstOrDefault();
            if (order == null)
            {
                return AppResult<OrderStatusHistoryWithShipmentDto>.Failure("Order not found", 404);
            }
            if (order.UserId != request.UserId)
            {
                return AppResult<OrderStatusHistoryWithShipmentDto>.Failure("Unauthorized access to order history", 403);
            }
            res.StatusHistory = order.StatusHistories == null ? new List<OrderStatusHistoryDto>() : _mapper.Map<List<OrderStatusHistoryDto>>(order.StatusHistories);
            res.Shipment = order.Shipment == null ? null : _mapper.Map<ShipmentDto>(order.Shipment);
            return AppResult<OrderStatusHistoryWithShipmentDto>.Success(res);
        }
    }
}