using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PhotoService;
using ProductService.Entities;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Banner
{
    public class DeleteBannerImageHandler : IRequestHandler<DeleteBannerImageCommand, AppResult<Unit>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;
        public DeleteBannerImageHandler(IProductUnitOfWork unitOfWork, IPhotoService photoService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }
        public async Task<AppResult<Unit>> Handle(DeleteBannerImageCommand request, CancellationToken cancellationToken)
        {
            foreach (var id in request.BannerImageIds)
            {
                var banner = await _unitOfWork.BannerImageRepository.GetByIdAsync(id);
                if (banner == null) return AppResult<Unit>.Failure($"Banner image with ID: {id} not found.", 404);
                _unitOfWork.BannerImageRepository.Delete(banner);
                await _photoService.DeleteAsync(banner.PublicId);
                var result = await _unitOfWork.CommitAsync();
                if (!result) return AppResult<Unit>.Failure($"Problem when delete banner image with ID: {id}.", 400);
            }
            
            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}