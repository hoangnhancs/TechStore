using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Persistence;
using ProductService.Repositories.Interface;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetTop10ProductPerCategoryHandler : IRequestHandler<GetTop10ProductPerCategoryQuery, AppResult<List<ProductDto>>>
{
    private readonly IProductUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public GetTop10ProductPerCategoryHandler(IMapper mapper, IProductUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<AppResult<List<ProductDto>>> Handle(GetTop10ProductPerCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.ProductRepository.GetTop10ProductPerCategory(cancellationToken);
        var productDtos = products.Select(_mapper.Map<ProductDto>).ToList();
        return AppResult<List<ProductDto>>.Success(productDtos);
    }
}
