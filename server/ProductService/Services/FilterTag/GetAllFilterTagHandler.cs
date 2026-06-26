using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.FilterTag
{
    public class GetAllFilterTagHandler : IRequestHandler<GetAllFilterTagQuery, AppResult<List<FilterTagDto>>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllFilterTagHandler(IProductUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppResult<List<FilterTagDto>>> Handle(GetAllFilterTagQuery request, CancellationToken cancellationToken)
        {
            var filterTags = await _unitOfWork.FilterTagRepository
                .GetByCategoryWithValuesAsync(request.CategoryId, cancellationToken);

            return AppResult<List<FilterTagDto>>.Success(_mapper.Map<List<FilterTagDto>>(filterTags));
        }
    }
}
