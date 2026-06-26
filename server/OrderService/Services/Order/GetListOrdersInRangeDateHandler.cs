using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;
using static OrderService.Entities.Order;

namespace OrderService.Services.Order;

public class GetListOrdersInRangeDateHandler : IRequestHandler<GetListOrdersInRangeDateQuery, AppResult<List<OrderWithUserInforDto>>>
{
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public GetListOrdersInRangeDateHandler(IOrderUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<AppResult<List<OrderWithUserInforDto>>> Handle(GetListOrdersInRangeDateQuery request, CancellationToken cancellationToken)
    {
         
        OrderStatus? enumStatus = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, out var parsed))
                return AppResult<List<OrderWithUserInforDto>>.Failure($"Invalid order status: '{request.Status}'", 400);
            enumStatus = parsed;
        }
        var orders = await _unitOfWork.OrderRepository.GetListOrdersInDateRangeWithUserInfor(request.StartDate, request.EndDate, enumStatus);
        var ordersDto = _mapper.Map<List<OrderWithUserInforDto>>(orders);
        return AppResult<List<OrderWithUserInforDto>>.Success(ordersDto);
    }
}
