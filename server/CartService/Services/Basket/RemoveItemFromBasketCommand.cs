using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket
{
    public class RemoveItemFromBasketCommand : IRequest<AppResult<Unit>>
    {
        public required string UserId { get; set; }
        public required string ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}