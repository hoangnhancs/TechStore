using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Brand
{
    public class GetAllBrandsQuery : IRequest<AppResult<List<BrandDto>>>
    {
        public int? CategoryId { get; set; }
    }
}