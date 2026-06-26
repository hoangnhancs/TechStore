using AutoMapper;
using CartService.DTOs;
using CartService.Persistence;
using MediatR;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket;

public class GetBasketHandler : IRequestHandler<GetBasketQuery, AppResult<BasketDto>>
{
    private readonly ICartUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly GrpcProductClient _grpcProductClient;

    public GetBasketHandler(IMapper mapper, ICartUnitOfWork unitOfWork, GrpcProductClient grpcProductClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _grpcProductClient = grpcProductClient;
    }

    public async Task<AppResult<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await _unitOfWork.BasketRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);

        if (basket == null)
        {
            basket = new Entities.Basket
            {
                UserId = request.UserId,
                Items = new List<Entities.BasketItem>()
            };

            await _unitOfWork.BasketRepository.AddAsync(basket, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        var dto = _mapper.Map<BasketDto>(basket);
        var productIds = basket.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _grpcProductClient.GetProducts(productIds, cancellationToken);
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var item in dto.Items)
        {
            if (!productDict.TryGetValue(item.ProductId, out var product)) continue;

            item.ProductName = product.Name;
            item.Price = product.Price;
            item.ImageUrl = product.MainImageUrl;
            item.BrandId = product.BrandId;
            item.BrandName = product.BrandName;
            item.CategoryId = product.CategoryId;
            item.CategoryName = product.CategoryName;
            item.CategoryDisplayName = product.CategoryDisplayName;
        }

        return AppResult<BasketDto>.Success(dto);
    }
}
