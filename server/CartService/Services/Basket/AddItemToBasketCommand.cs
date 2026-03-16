using System;
using System.Text.Json.Serialization;
using CartService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket;

public class AddItemToBasketCommand : IRequest<AppResult<BasketDto>>
{
    [JsonIgnore] 
    public string? UserId { get; set; }
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }
}
