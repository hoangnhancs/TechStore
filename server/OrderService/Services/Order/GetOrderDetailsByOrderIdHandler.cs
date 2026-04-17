using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

public class GetOrderDetailsByOrderIdHandler : IRequestHandler<GetOrderDetailsByOrderIdQuery, AppResult<OrderDto>>
{
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderDetailsByOrderIdHandler(IMapper mapper, IOrderUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<OrderDto>> Handle(GetOrderDetailsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.OrderRepository.GetByIdAsync(
            request.OrderId, 
            q => q.Include(o => o.Items)
                .Include(o => o.Shipment)
                .Include(o => o.StatusHistories),
            cancellationToken);
        if (order == null) return AppResult<OrderDto>.Failure("Order not found", 404);
        if (order.UserId != request.UserId) return AppResult<OrderDto>.Failure("Unauthorized access to order details", 403);
        return AppResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
}

