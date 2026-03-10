using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product
{
    public class UpdateProductCommand : IRequest<AppResult<ProductDto>>
    {
        public required UpdateProductDto ProductDto { get; set; }
        public required string ProductId { get; set; }
    }
}