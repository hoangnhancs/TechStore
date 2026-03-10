using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Banner
{
    public class GetAllBannerImagesHandler : IRequestHandler<GetAllBannerImagesQuery, AppResult<List<BannerImageDto>>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetAllBannerImagesHandler(IProductUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<List<BannerImageDto>>> Handle(GetAllBannerImagesQuery request, CancellationToken cancellationToken)
        {
            var bannerImages = await _unitOfWork.BannerImageRepository.GetListAsync(cancellationToken: cancellationToken);
            return AppResult<List<BannerImageDto>>.Success(_mapper.Map<List<BannerImageDto>>(bannerImages));
        }
    }
}