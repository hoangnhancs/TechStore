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

namespace OrderService.Services.Order
{
    public class GetOrderWithStatusHistoryAndShipmentHandler : IRequestHandler<GetOrderWithStatusHistoryAndShipmentQuery, AppResult<OrderDto>>
    {
        private readonly IOrderUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetOrderWithStatusHistoryAndShipmentHandler(IOrderUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<OrderDto>> Handle(GetOrderWithStatusHistoryAndShipmentQuery request, CancellationToken cancellationToken)
        {
            var order = (await _unitOfWork.OrderRepository.GetListAsync(
                p => p.Id == request.OrderId,
                q => q.Include(o => o.StatusHistories).Include(o => o.Shipment).Include(o => o.Items)
            )).FirstOrDefault();
            if (order == null)
            {
                return AppResult<OrderDto>.Failure("Order not found", 404);
            }
            if (order.UserId != request.UserId)
            {
                return AppResult<OrderDto>.Failure("Unauthorized access to order history", 403);
            }
            var res = _mapper.Map<OrderDto>(order);
            return AppResult<OrderDto>.Success(res);
        }
    }
}