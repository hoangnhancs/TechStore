using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            var filterTags = await _unitOfWork.FilterTagRepository.GetListAsync(
                p => !request.CategoryId.HasValue || p.CategoryId == request.CategoryId.Value,
                q => q.Include(ft => ft.Values),
                cancellationToken);
            var filterTagDtos = _mapper.Map<List<FilterTagDto>>(filterTags);
            return AppResult<List<FilterTagDto>>.Success(filterTagDtos);
        }
    }
}