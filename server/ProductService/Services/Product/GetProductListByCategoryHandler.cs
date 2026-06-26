using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Persistence;
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
            var products = await _unitOfWork.ProductRepository
                .GetActiveByCategoryOrBrandAsync(request.CategoryId, request.BrandId, cancellationToken);

            if (!products.Any())
                return AppResult<List<ProductDto>>.Success([]);

            return AppResult<List<ProductDto>>.Success(products.Select(_mapper.Map<ProductDto>).ToList());
        }
    }
}
