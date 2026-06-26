using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

public class GetListOrdersByUserIdHandler : IRequestHandler<GetListOrdersByUserIdQuery, AppResult<List<OrderDto>>>
{
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public GetListOrdersByUserIdHandler(IMapper mapper, IOrderUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;

    }

    public async Task<AppResult<List<OrderDto>>> Handle(GetListOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        var order = (await _unitOfWork.OrderRepository.GetListAsync(
            p => p.UserId == request.UserId,
            q => q.Include(o => o.Items),
            cancellationToken
        )).OrderByDescending(o => o.CreatedAt).ToList();
        if (order == null || order.Count == 0)
        {
            return AppResult<List<OrderDto>>.Success([]);
        }

        return AppResult<List<OrderDto>>.Success(order.Select(_mapper.Map<OrderDto>).ToList());
    }
}
