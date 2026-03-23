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
        var userId = User.FindFirst("sub")?.Value 
                     ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var command = new CreateOrderCommand 
        { 
            UserId = userId,
            CreateOrderDto = dto 
        };

        return HandleAppResult(await Mediator.Send(command));
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetOrdersByUserId(string userId)
    {
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
