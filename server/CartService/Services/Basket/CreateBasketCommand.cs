using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket
{
    public class CreateBasketCommand : IRequest<AppResult<BasketDto>>
    {
        public required string UserId { get; set; }
    }
}