using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product
{
    public class CreateNewProductCommand : IRequest<AppResult<ProductDto>>
    {
        public required CreateProductDto ProductDto { get; set; }
    }
}