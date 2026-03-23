using MediatR;
using OrderService.DTOs;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

/// <summary>
/// Command to create a new order
/// </summary>
public class CreateOrderCommand : IRequest<AppResult<OrderDto>>
{
    public required string UserId { get; set; }
    public required CreateOrderDto CreateOrderDto { get; set; }
}
