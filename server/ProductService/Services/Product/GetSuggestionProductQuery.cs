using System;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetSuggestionProductQuery : IRequest<AppResult<List<ProductDto>>>
{
    public string? UserId { get; set; }
}
