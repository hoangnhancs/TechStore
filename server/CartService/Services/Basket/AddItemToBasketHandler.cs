using System;
using AutoMapper;
using CartService.DTOs;
using CartService.Persistence;
using CartService.Services.Basket;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var basket = (await _unitOfWork.BasketRepository.GetListAsync(
            p => p.UserId == request.UserId,
            q => q.Include(b => b.Items),
            cancellationToken: cancellationToken
        )).FirstOrDefault();

        var isNewBasket = false;
        if (basket == null)
        {
            basket = new Entities.Basket
            {
                UserId = request.UserId,
                Items = new List<Entities.BasketItem>()
            };
            isNewBasket = true;
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
            if (product.QuantityInStock < request.Quantity)
            {
                return AppResult<BasketDto>.Failure("Insufficient stock", 400);
            }
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


        basket.AddItem(request.ProductId, request.Quantity);

        if (isNewBasket)
        {
            await _unitOfWork.BasketRepository.AddAsync(basket, cancellationToken);
        }

        // await _userActionTrackingRepository.AddUserActionTracking(new UserActionTracking
        // {
        //     UserId = request.UserId,
        //     ProductId = request.ProductId,
        //     ActionType = UserActionTracking.UserActionType.AddToCart
        // }, cancellationToken);
        try
        {
            var result = await _unitOfWork.CommitAsync(cancellationToken);
            if (!result)
            {
                _logger.LogWarning(
                    "CommitAsync returned false while adding product {ProductId} to basket for user {UserId}. IsNewBasket: {IsNewBasket}",
                    request.ProductId,
                    request.UserId,
                    isNewBasket);
                return AppResult<BasketDto>.Failure("No changes were saved to basket.", 400);
            }

            return AppResult<BasketDto>.Success(_mapper.Map<BasketDto>(basket));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while adding product {ProductId} to basket for user {UserId}", request.ProductId, request.UserId);
            return AppResult<BasketDto>.Failure("Error updating basket. Please try again.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding product {ProductId} to basket for user {UserId}", request.ProductId, request.UserId);
            return AppResult<BasketDto>.Failure("An unexpected error occurred. Please try again.", 500);
        }
    }
}


