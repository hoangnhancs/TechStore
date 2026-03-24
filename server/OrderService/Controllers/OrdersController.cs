using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services.Order;
using Shared.Web.Controller;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var command = new CreateOrderCommand 
        { 
            UserId = userId,
            CreateOrderDto = dto 
        };

        return HandleAppResult(await Mediator.Send(command));
    }

    [HttpPut("{id}/process")]
    public async Task<IActionResult> Process(string id)
    {
        return HandleAppResult(await Mediator.Send(new UpdateOrderStatusCommand
        {
            OrderId = id.ToString(),
            NewStatus = Entities.Order.OrderStatus.Processing
        }));
    }

    [HttpPut("{id}/ship")]
    public async Task<IActionResult> Ship(string id)
    {
        return HandleAppResult(await Mediator.Send(new UpdateOrderStatusCommand
        {
            OrderId = id.ToString(),
            NewStatus = Entities.Order.OrderStatus.Shipped
        }));
    }

    [HttpPut("{id}/deliver")]
    public async Task<IActionResult> Deliver(string id)
    {
        return HandleAppResult(await Mediator.Send(new UpdateOrderStatusCommand
        {
            OrderId = id.ToString(),
            NewStatus = Entities.Order.OrderStatus.Delivered
        }));
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(string id)
    {
        return HandleAppResult(await Mediator.Send(new UpdateOrderStatusCommand
        {
            OrderId = id.ToString(),
            NewStatus = Entities.Order.OrderStatus.Completed
        }));
    }


    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(string id)
    {
        return HandleAppResult(await Mediator.Send(new UpdateOrderStatusCommand
        {
            OrderId = id.ToString(),
            NewStatus = Entities.Order.OrderStatus.Cancelled
        }));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrdersByUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var query = new GetListOrdersByUserIdQuery { UserId = userId };
        return HandleAppResult(await Mediator.Send(query));
    }

    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderDetails(string orderId)
    {
        var query = new GetOrderDetailsByOrderIdQuery { OrderId = orderId };
        return HandleAppResult(await Mediator.Send(query));
    }

    [HttpGet("range")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrdersInRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var query = new GetListOrdersInRangeDateQuery 
        { 
            StartDate = startDate, 
            EndDate = endDate 
        };
        return HandleAppResult(await Mediator.Send(query));
    }
}
