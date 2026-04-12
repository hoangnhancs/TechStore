using System;
using MediatR;
using OrderService.DTOs;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

public class GetOrderDetailsByOrderIdQuery : IRequest<AppResult<OrderDto>>
{
    public required string UserId { get; set; }
    public required string OrderId { get; set; }
}
