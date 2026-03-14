using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contract;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotoService;
using PhotoService.DTOs;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, AppResult<ProductDto>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateProductHandler(IProductUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<AppResult<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
        
            var dto = request.ProductDto;
            if (request.ProductId == null) return AppResult<ProductDto>.Failure("Product ID cannot be null or empty", 400);
            var existingProduct = await _unitOfWork.ProductRepository.GetByIdAsync(
                request.ProductId,
                q => q.Include(p => p.DetailImages)
                    .Include(p => p.Attributes)
                    .Include(p => p.ProductFilterTagValues)
            );
            if (existingProduct == null) return AppResult<ProductDto>.Failure("Product not found", 404);

            string desPath = "TechStore";
            var oldMainImagePublicId = existingProduct.MainImagePublicId;
            var oldMainImageUrl = existingProduct.MainImageUrl;
            var oldDetailImages = existingProduct.DetailImages.ToList();


            // 1. Upload all new images sequentially with retry, rollback if any fail
            PhotoUploadResultDto? uploadedMainImage = null;
            var uploadedDetailImages = new List<PhotoUploadResultDto>();
            var uploadedPublicIds = new List<string>();
            var deletePublicIds = new List<string>();

            

            // Main image
            if (dto.MainImageFile != null && dto.MainImageFile.Length > 0)
            {
                var mainImgUplRes = await TryUploadAsync(async () => {
                    var r = await _photoService.UploadAsync(dto.MainImageFile, desPath);
                    return r == null ? null : r;
                });

                if (mainImgUplRes == null)
                {
                    return AppResult<ProductDto>.Failure("Main image upload failed after 3 attempts", 502);
                }
                else 
                {
                    uploadedMainImage = mainImgUplRes; 
                    uploadedPublicIds.Add(mainImgUplRes.PublicId);         
                }
            }

            if (dto.DetailImageFiles != null && dto.DetailImageFiles.Count > 0)
            {
                foreach (var img in dto.DetailImageFiles)
                {
                    var uplRes = await TryUploadAsync(async () => {
                        var r = await _photoService.UploadAsync(img, desPath);
                        return r == null ? null : r;
                    });
                    if (uplRes == null)
                    {
                        // Rollback any uploaded images
                        foreach (var pid in uploadedPublicIds)
                            await _photoService.DeleteAsync(pid);
                        return AppResult<ProductDto>.Failure($"Detail image upload failed for image: {img.Name} after 3 attempts", 502);
                    }
                    else 
                    {
                        uploadedDetailImages.Add(uplRes);
                        uploadedPublicIds.Add(uplRes.PublicId);
                    }
                }
            }

            try
            {
                _mapper.Map(dto, existingProduct);

                if (uploadedMainImage != null) //gan anh moi neu co upload
                {
                    existingProduct.MainImageUrl = uploadedMainImage.Url;
                    existingProduct.MainImagePublicId = uploadedMainImage.PublicId;
                    deletePublicIds.Add(oldMainImagePublicId);
                }
                else //giu anh cu
                {
                    existingProduct.MainImageUrl = oldMainImageUrl;
                    existingProduct.MainImagePublicId = oldMainImagePublicId;
                }

                existingProduct.DetailImages.Clear();
                existingProduct.ProductFilterTagValues.Clear();
                existingProduct.Attributes.Clear();

                foreach (var image in oldDetailImages) //check xem co trong dto.DetailImageUrls ko
                //neu co thi giu lai, ko co thi xoa
                {
                    if (dto.DetailImageUrls != null && dto.DetailImageUrls.Contains(image.ImageUrl) && !string.IsNullOrEmpty(image.PublicId))
                    {
                        existingProduct.DetailImages.Add(image);
                    }
                    else 
                    {
                        deletePublicIds.Add(image.PublicId);
                    }
                }

                foreach (var img in uploadedDetailImages) //them anh moi upload
                {
                    var newDetailImage = new ProductImage
                    {
                        ImageUrl = img.Url,
                        PublicId = img.PublicId,
                        ProductId = existingProduct.Id
                    };
                    existingProduct.DetailImages.Add(newDetailImage);
                }

                if (dto.ProductFilterTagValues != null)
                {
                    foreach (var ftv in dto.ProductFilterTagValues)
                    {
                        existingProduct.ProductFilterTagValues.Add(new ProductFilterTagValue
                        {
                            FilterTagValueId = ftv.FilterTagValueId,
                            ProductId = existingProduct.Id
                        });
                    }
                }

                if (dto.Attributes != null)
                {
                    int displayOrder = 0;
                    foreach (var attr in dto.Attributes)
                    {
                        existingProduct.Attributes.Add(new ProductAttribute
                        {
                            Name = attr.Name,
                            Value = attr.Value,
                            ProductId = existingProduct.Id,
                            DisplayOrder = displayOrder,
                            AttributeType = attr.AttributeType
                        });
                        displayOrder++;
                    }
                }

                _unitOfWork.ProductRepository.Update(existingProduct);
                var res = await _unitOfWork.CommitAsync();

                if (!res)
                {
                    // Rollback: remove all uploaded images
                    foreach (var pid in uploadedPublicIds)
                        await _photoService.DeleteAsync(pid);
                    return AppResult<ProductDto>.Failure("Failed to update product", 500);
                }
                else 
                {
                    // Delete old images only after DB commit
                    foreach (var pid in deletePublicIds)
                    {
                        if (!string.IsNullOrEmpty(pid))
                        {
                            await _photoService.DeleteAsync(pid);
                        }
                    }
                }
                var productUpdated = _mapper.Map<ProductUpdated>(existingProduct);
                await _publishEndpoint.Publish(productUpdated, cancellationToken);
                return AppResult<ProductDto>.Success(_mapper.Map<ProductDto>(existingProduct));
            }
            catch (Exception ex)
            {
                
                foreach (var pid in uploadedPublicIds)
                    await _photoService.DeleteAsync(pid);
                return AppResult<ProductDto>.Failure($"Update failed: {ex.Message}", 500);
            }
        }

        public async Task<PhotoUploadResultDto?> TryUploadAsync(Func<Task<PhotoUploadResultDto?>> uploadFunc, int maxTries = 3)
        {
            for (int i = 0; i < maxTries; i++)
            {
                var result = await uploadFunc();
                if (result != null) return result;
            }
            return null;
        }
    }
}