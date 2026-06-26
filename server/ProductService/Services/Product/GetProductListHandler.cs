using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class GetProductListHandler : IRequestHandler<GetProductListQuery, AppResult<List<ProductDto>>>
{
    private readonly IProductUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductListHandler(IMapper mapper, IProductUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppResult<List<ProductDto>>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
    {
        DateTime? lastUpdatedDate = null;
        if (!string.IsNullOrEmpty(request.LastUpdated))
        {
            if (DateTime.TryParse(request.LastUpdated, out var parsedDate))
                lastUpdatedDate = parsedDate.ToUniversalTime();
        }
        else
        {
            lastUpdatedDate = DateTime.MinValue;
        }

        var products = await _unitOfWork.ProductRepository.GetActiveProductsAsync(lastUpdatedDate, cancellationToken);
        var productsDto = products.Select(_mapper.Map<ProductDto>).ToList();

        return AppResult<List<ProductDto>>.Success(productsDto);
    }
}
