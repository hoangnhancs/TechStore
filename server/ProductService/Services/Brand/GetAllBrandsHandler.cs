using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Brand
{
    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, AppResult<List<BrandDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IProductUnitOfWork _unitOfWork;

        public GetAllBrandsHandler(IMapper mapper, IProductUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppResult<List<BrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var brands = await _unitOfWork.BrandRepository.GetByCategoryAsync(request.CategoryId, cancellationToken);
            return AppResult<List<BrandDto>>.Success(_mapper.Map<List<BrandDto>>(brands));
        }
    }
}
