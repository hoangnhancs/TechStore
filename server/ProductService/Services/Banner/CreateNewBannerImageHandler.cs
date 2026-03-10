using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PhotoService;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Banner
{
    public class CreateNewBannerImageHandler : IRequestHandler<CreateNewBannerImageCommand, AppResult<List<BannerImageDto>>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;
        public CreateNewBannerImageHandler(IProductUnitOfWork unitOfWork, IPhotoService photoService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        public async Task<AppResult<List<BannerImageDto>>> Handle(CreateNewBannerImageCommand request, CancellationToken cancellationToken)
        {
            var bannerImagesDto = new List<BannerImageDto>();
            foreach (var image in request.NewImages)
            {
                var uploadResult = await _photoService.UploadAsync(image, "TechStore/banners");
                if (uploadResult == null) return AppResult<List<BannerImageDto>>.Failure($"Image {image.Name} upload failed", 502);
                var bannerImage = new BannerImage
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url
                };
                await _unitOfWork.BannerImageRepository.AddAsync(bannerImage, cancellationToken);
                var result = await _unitOfWork.CommitAsync();
                if (!result) return AppResult<List<BannerImageDto>>.Failure($"Problem when create banner image {image.Name}", 400);
                bannerImagesDto.Add(_mapper.Map<BannerImageDto>(bannerImage));
            }
            
            return AppResult<List<BannerImageDto>>.Success(bannerImagesDto);
        }
    }
}