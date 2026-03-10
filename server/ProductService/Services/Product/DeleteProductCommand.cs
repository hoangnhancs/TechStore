using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product
{
    public class DeleteProductCommand : IRequest<AppResult<Unit>>
    {
        public required string ProductId { get; set; }
    }
}