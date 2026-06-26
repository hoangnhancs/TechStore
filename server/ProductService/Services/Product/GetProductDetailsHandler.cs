using AutoMapper;
using MediatR;
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
        var product = await _unitOfWork.ProductRepository.GetProductForDisplayAsync(request.ProductId, cancellationToken);

        if (product == null)
            return AppResult<ProductDto?>.Failure("Product not found", 404);

        return AppResult<ProductDto?>.Success(_mapper.Map<ProductDto>(product));
    }
}
