using System;
using MediatR;
using ProductService.Persistence;
using Shared.Core.EF.Application;

namespace ProductService.Services.Product;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, AppResult<Unit>>
{
    private readonly IProductUnitOfWork _unitOfWork;
    // private readonly IProductRepository _productRepository;
    // private readonly IPhotoService _photoService;
    public DeleteProductHandler(IProductUnitOfWork unitOfWork /*IProductRepository productRepository, IPhotoService photoService*/)
    {
        _unitOfWork = unitOfWork;
        // _productRepository = productRepository;
        // _photoService = photoService;
    }
    public async Task<AppResult<Unit>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId, null, cancellationToken);
        if (product == null) return AppResult<Unit>.Failure("Product not found", 404);
        product.IsActive = false;
        product.MarkAsDeleted();    
        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.CommitAsync();
        if (!string.IsNullOrEmpty(product.MainImagePublicId))
        {
            // await _photoService.DeletePhoto(product.MainImagePublicId);
            //chi nhung product moi, moi upload anh len cloudinary\
            //check publicID neu valid thi xoa
        }
        if (product.DetailImages != null && product.DetailImages.Count > 0)
        {
            foreach (var image in product.DetailImages)
            {
                if (!string.IsNullOrEmpty(image.PublicId))
                {
                    // await _photoService.DeletePhoto(image.PublicId);
                }
            }
            //ims detail cung valid thi xoa
        }
         
        return AppResult<Unit>.Success(Unit.Value);
    }
}
