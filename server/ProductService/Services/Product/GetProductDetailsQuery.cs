using System;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetProductDetailsQuery : IRequest<AppResult<ProductDto?>>
{
    public required string ProductId { get; set; }
}
