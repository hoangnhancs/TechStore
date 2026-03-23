using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

public class GetListOrdersInRangeDateHandler : IRequestHandler<GetListOrdersInRangeDateQuery, AppResult<List<OrderDto>>>
{
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public GetListOrdersInRangeDateHandler(IOrderUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<AppResult<List<OrderDto>>> Handle(GetListOrdersInRangeDateQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.OrderRepository.GetListAsync(
            p => p.CreatedAt >= request.StartDate && p.CreatedAt <= request.EndDate,
            q => q.Include(o => o.Items),
            cancellationToken
        );
        var ordersDto = orders.Select(_mapper.Map<OrderDto>).ToList();
        return AppResult<List<OrderDto>>.Success(ordersDto);
    }
}
