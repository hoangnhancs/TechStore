using System;
using CartService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket;

public class GetBasketQuery : IRequest<AppResult<BasketDto>>
{
    public required string UserId { get; set; }
}
