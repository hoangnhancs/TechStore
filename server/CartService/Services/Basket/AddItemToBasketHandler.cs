using System;
using AutoMapper;
using CartService.DTOs;
using CartService.Persistence;
using CartService.Services.Basket;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.EF.Application;

namespace CartService.Services.Basket;

public class AddItemToBasketHandler : IRequestHandler<AddItemToBasketCommand, AppResult<BasketDto>>
{
    private readonly ICartUnitOfWork _unitOfWork;
    private readonly GrpcProductClient _grpcProductClient;
    private readonly ILogger<AddItemToBasketHandler> _logger;
    // private readonly IUserActionTrackingRepository _userActionTrackingRepository;
    private readonly IMapper _mapper;

    public AddItemToBasketHandler(
        ICartUnitOfWork unitOfWork, 
        GrpcProductClient grpcProductClient, 
        ILogger<AddItemToBasketHandler> logger,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _grpcProductClient = grpcProductClient;
        _logger = logger;
        _mapper = mapper;
        // _userActionTrackingRepository = userActionTrackingRepository;
    }

    public async Task<AppResult<BasketDto>> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return AppResult<BasketDto>.Failure("User ID cannot be null or empty", 400);
        }

        // Validate product exists via gRPC
        try
        {
            var product = await _grpcProductClient.GetProduct(request.ProductId, cancellationToken);
            
            if (product == null)
            {
                return AppResult<BasketDto>.Failure("Product not found", 404);
            }

            // Optional: Validate product stock if needed
            // if (product.Stock < request.Quantity)
            // {
            //     return AppResult<BasketDto>.Failure("Insufficient stock", 400);
            // }
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout validating product {ProductId} for user {UserId}", request.ProductId, request.UserId);
            return AppResult<BasketDto>.Failure("Product service is temporarily unavailable. Please try again.", 503);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product {ProductId} for user {UserId}", request.ProductId, request.UserId);
            return AppResult<BasketDto>.Failure("Error validating product. Please try again.", 500);
        }

        var newBasket = new Entities.Basket
        {
            UserId = request.UserId
        };
        newBasket.AddItem(request.ProductId, request.Quantity);
        
        await _unitOfWork.BasketRepository.AddAsync(newBasket, cancellationToken);

        // await _userActionTrackingRepository.AddUserActionTracking(new UserActionTracking
        // {
        //     UserId = request.UserId,
        //     ProductId = request.ProductId,
        //     ActionType = UserActionTracking.UserActionType.AddToCart
        // }, cancellationToken);

        var result = await _unitOfWork.CommitAsync(cancellationToken);

        if (!result) return AppResult<BasketDto>.Failure("Don't have any update when add item", 400);

        

        return AppResult<BasketDto>.Success(_mapper.Map<BasketDto>(newBasket));
    }
}


