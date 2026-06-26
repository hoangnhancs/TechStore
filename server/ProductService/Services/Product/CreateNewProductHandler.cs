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
    public class CreateNewProductHandler : IRequestHandler<CreateNewProductCommand, AppResult<ProductDto>>
    {
        private readonly IProductUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IPhotoService _photoService;
        public CreateNewProductHandler(IProductUnitOfWork unitOfWork, IMapper mapper, IPublishEndpoint publishEndpoint, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _photoService = photoService;
        }
        public async Task<AppResult<ProductDto>> Handle(CreateNewProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto;
            var product = _mapper.Map<Entities.Product>(dto); //mapping ca attributes va productfiltertagvalues
            
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return AppResult<ProductDto>.Failure("Category not found", 404);
            }

            string productFolder = dto.Name.Replace(" ", "-").ToLower();
            string desFolder = $"TechStore/{category.Name}/{productFolder}";
            
            var mainImageUploadResult = await _photoService.UploadAsync(dto.MainImageFile, desFolder);
            if (mainImageUploadResult == null)
            {
                throw new Exception("Failed to upload main image");
            }
            var detailImageUploadResults = new List<PhotoUploadResultDto>();
            foreach (var detailImageFile in dto.DetailImageFiles)
            {
                var detailImageUploadResult = await _photoService.UploadAsync(detailImageFile, desFolder);
                detailImageUploadResults.Add(detailImageUploadResult ?? throw new Exception("Failed to upload detail image"));
            }

            var newProductId = Guid.NewGuid().ToString(); //Taạo id mới

            product.Id = newProductId; //Gán
            product.MainImageUrl = mainImageUploadResult.Url;
            product.MainImagePublicId = mainImageUploadResult.PublicId;

            var newListProductImages = new List<ProductImage>();
            foreach (var detailImageUploadResult in detailImageUploadResults)
            {
                newListProductImages.Add(new ProductImage
                {
                    ImageUrl = detailImageUploadResult.Url,
                    PublicId = detailImageUploadResult.PublicId,
                    ProductId = newProductId //Gán id sản phẩm mới
                });
            }
            product.DetailImages = newListProductImages;

            for(int i =0; i < product.Attributes.Count; i++)
            {
                product.Attributes[i].DisplayOrder = i;
            }

            await _unitOfWork.ProductRepository.AddAsync(product, cancellationToken);
            
            var result = await _unitOfWork.CommitAsync();
            if (result)
            {
                // Reload product with navigation properties for event publishing
                var savedProduct = await _unitOfWork.ProductRepository.GetProductWithDetailsAsync(newProductId, cancellationToken);

                if (savedProduct != null)
                {
                    var productCreated = _mapper.Map<ProductCreated>(savedProduct);
                    await _publishEndpoint.Publish(productCreated, cancellationToken);
                    await _unitOfWork.CommitAsync();
                }

                var productDtoResult = _mapper.Map<ProductDto>(product);
                return AppResult<ProductDto>.Success(productDtoResult);
            }
            return AppResult<ProductDto>.Failure("Failed to create new product", 500);
        }
    }
}