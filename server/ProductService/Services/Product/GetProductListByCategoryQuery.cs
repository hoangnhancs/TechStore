using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product
{
    public class GetProductListByCategoryQuery : IRequest<AppResult<List<ProductDto>>>
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
    }
}