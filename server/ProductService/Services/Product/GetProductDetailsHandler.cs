using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetProductDetailsHandler : IRequestHandler<GetProductDetailsQuery, AppResult<ProductDto?>>
{
    private readonly IProductUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;


    public GetProductDetailsHandler(IMapper mapper, IProductUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<ProductDto?>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(
            request.ProductId, 
            q => q.Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.DisplayTags)
                .Include(p => p.ProductFilterTagValues)
                .ThenInclude(pftv => pftv.FilterTagValue)
                .Include(p => p.DetailImages)
                .Include(p => p.Attributes),
            cancellationToken
        );
        // var product = await _repository.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
        {
            return AppResult<ProductDto?>.Failure("Product not found", 404);
        }

        var dto = _mapper.Map<ProductDto>(product);
        return AppResult<ProductDto?>.Success(dto);
    }
}

