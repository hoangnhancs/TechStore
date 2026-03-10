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

namespace ProductService.Services.Category
{
    public class GetCategoryListHandler : IRequestHandler<GetCategoryListQuery, AppResult<List<CategoryDto>>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetCategoryListHandler(IProductUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<List<CategoryDto>>> Handle(GetCategoryListQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.CategoryRepository.GetListAsync(
                cancellationToken: cancellationToken);
            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return AppResult<List<CategoryDto>>.Success(categoryDtos);
        }
    }
}