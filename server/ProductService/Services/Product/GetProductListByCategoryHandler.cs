using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Persistence;
using ProductService.Repositories;
using ProductService.Repositories.Interface;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product
{
    public class GetProductListByCategoryHandler : IRequestHandler<GetProductListByCategoryQuery, AppResult<List<ProductDto>>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetProductListByCategoryHandler(IProductUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<List<ProductDto>>> Handle(GetProductListByCategoryQuery request, CancellationToken cancellationToken)
        {
            var products = await _unitOfWork.ProductRepository.GetListAsync(
                p => (request.CategoryId == null || p.CategoryId == request.CategoryId) 
                    && (request.BrandId == null || p.BrandId == request.BrandId) 
                    && p.IsActive,  
                q => q.Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.DisplayTags)
                    .Include(p => p.ProductFilterTagValues),
                cancellationToken
            );
            // var products = await _productRepository.GetProductsByCategory(request.CategoryId, cancellationToken);
            if (products == null || products.Count() == 0)
            {
                return AppResult<List<ProductDto>>.Success(new List<ProductDto>());
            }
            var productsDto = products.Select(_mapper.Map<ProductDto>).ToList();
            return AppResult<List<ProductDto>>.Success(productsDto);
        }
    }
}