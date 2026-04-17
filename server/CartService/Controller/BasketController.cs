using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CartService.DTOs;
using CartService.Services.Basket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Web.Controller;

namespace CartService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetBasket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            return HandleAppResult(await Mediator.Send(new GetBasketQuery { UserId = userId }));
        }

        [HttpPost("mybasket/items")]
        public async Task<IActionResult> AddItemToBasket(AddItemDto addItemDto)
        {
            var identity = User.Identity;
            var isAuthenticated = identity?.IsAuthenticated ?? false;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "User not authenticated",
                    isAuthenticated,
                    claims,
                    identityName = identity?.Name
                });
            }

            return HandleAppResult(await Mediator.Send(new AddItemToBasketCommand
            {
                UserId = userId,
                ProductId = addItemDto.ProductId,
                Quantity = addItemDto.Quantity
            }));
        }

        [HttpPut("mybasket/items")]
        public async Task<IActionResult> RemoveItemFromBasket(RemoveItemDto removeItemDto)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            return HandleAppResult(await Mediator.Send(new RemoveItemFromBasketCommand
            {
                UserId = userId,
                ProductId = removeItemDto.ProductId,
                Quantity = removeItemDto.Quantity
            }));
        }

        [HttpPost("mybasket/remove_items")]
        public async Task<IActionResult> RemovePermanentItemsFromBasket([FromBody] RemovePermanentItemsDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            return HandleAppResult(await Mediator.Send(new RemovePermanentItemsFromBasketCommand
            {
                UserId = userId,
                ProductIds = dto.ProductIds
            }));
        }
    }
}