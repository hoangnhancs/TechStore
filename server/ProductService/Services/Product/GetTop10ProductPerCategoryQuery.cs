using System;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetTop10ProductPerCategoryQuery : IRequest<AppResult<List<ProductDto>>>
{

}
