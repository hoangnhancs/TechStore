using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Persistence;
using Shared.Core.EF.Application;

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
        var orders = await _unitOfWork.OrderRepository.GetListOrdersInDateRangeWithUserInfor(request.StartDate, request.EndDate);
        var ordersDto = _mapper.Map<List<OrderWithUserInforDto>>(orders);
        return AppResult<List<OrderWithUserInforDto>>.Success(ordersDto);
    }
}
