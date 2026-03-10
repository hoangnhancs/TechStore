using System;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetProductListQuery : IRequest<AppResult<List<ProductDto>>>
{
    public string? LastUpdated { get; set; }
}

