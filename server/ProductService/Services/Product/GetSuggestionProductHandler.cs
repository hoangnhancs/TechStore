using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetSuggestionProductHandler : IRequestHandler<GetSuggestionProductQuery, AppResult<List<ProductDto>>>
{
    public Task<AppResult<List<ProductDto>>> Handle(GetSuggestionProductQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
