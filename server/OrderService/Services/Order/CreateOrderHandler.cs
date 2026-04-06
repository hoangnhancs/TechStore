using System.Security.Claims;
using AutoMapper;
using Contract;
using EmailService.Interfaces;
using EmailService.Services.Interface;
using MassTransit;
using MediatR;
using OrderService.DTOs;
using OrderService.Entities;
using OrderService.Persistence;
// using ProductService; // Commented: Using Saga pattern instead of direct gRPC
using Shared.Core.EF.Application;
using static OrderService.Entities.Order;

namespace OrderService.Services.Order;

/// <summary>
/// Handler for creating a new order with Saga orchestration
/// Uses event-driven pattern with Outbox for reliability
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, AppResult<OrderDto>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    // private readonly GrpcProduct.GrpcProductClient _productGrpcClient; // Commented: Using Saga 
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IHttpContextAccessor httpContextAccessor,
        IOrderUnitOfWork unitOfWork, 
        IMapper mapper,
        // GrpcProduct.GrpcProductClient productGrpcClient, // Commented: Using Saga
        IPublishEndpoint publishEndpoint,
        ILogger<CreateOrderHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        // _productGrpcClient = productGrpcClient; // Commented: Using Saga
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AppResult<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.CreateOrderDto;

            // Step 1: Validate items
            if (dto.Items == null || dto.Items.Count == 0)
                return AppResult<OrderDto>.Failure("Order must have at least one item", 400);

            _logger.LogInformation("Creating order for user {UserId} with {ItemCount} items", 
                request.UserId, dto.Items.Count);

            /* COMMENTED: gRPC sync call - Now using Saga pattern
            // Step 2: Reserve stock via gRPC to ProductService
            var reserveStockRequest = new ReserveStockRequest();
            reserveStockRequest.Items.AddRange(dto.Items.Select(item => new ReserveStockItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }));

            ReserveStockResponse stockResponse;
            try
            {
                stockResponse = await _productGrpcClient.ReserveStockAsync(
                    reserveStockRequest, 
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call ProductService gRPC ReserveStock");
                return AppResult<OrderDto>.Failure(
                    "Unable to verify product stock availability. Please try again later.", 500);
            }

            // Step 3: Check if stock reservation was successful
            if (!stockResponse.Success)
            {
                var failedItems = stockResponse.Results
                    .Where(r => !r.Success)
                    .Select(r => $"{r.ProductName}: requested {r.RequestedQuantity}, available {r.AvailableQuantity}")
                    .ToList();

                var errorMessage = $"Stock reservation failed. {stockResponse.ErrorMessage}\n" +
                                   string.Join("\n", failedItems);

                _logger.LogWarning("Stock reservation failed for order: {ErrorMessage}", errorMessage);
                return AppResult<OrderDto>.Failure(errorMessage, 400);
            }

            _logger.LogInformation("Stock reserved successfully, creating order in database");
            */

            // Step 2: Create order entity in "Pending" status
            // Saga will handle stock reservation asynchronously
            // Step 2: Create order entity in "Pending" status
            // Saga will handle stock reservation asynchronously
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            var phone = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.MobilePhone)?.Value;
            var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, ignoreCase: true, out var paymentMethod))
            {
                return AppResult<OrderDto>.Failure("Invalid payment method", 400);
            }

            var orderItems = dto.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                OrderId = string.Empty // Will be set after order is created
            }).ToList();

            var order = Entities.Order.CreateOrder(
                userId: userId ?? throw new ArgumentException("User ID not found in token"),
                userName: username ?? throw new ArgumentException("Username not found in token"),
                userEmail: email,
                userPhone: phone ?? throw new ArgumentException("User phone not found in token"),
                items: orderItems,
                shippingAddress: dto.ShippingAddress,
                billingAddress: dto.BillingAddress,
                shippingCost: dto.ShippingCost,
                discount: dto.Discount,
                paymentMethod: paymentMethod
            );

            // Set order reference for items
            foreach (var item in order.Items)
            {
                item.OrderId = order.Id;
            }

            // Step 3: Prepare OrderCreated event BEFORE committing
            // UseBusOutbox() requires Publish to be called before SaveChangesAsync
            // so the outbox message is written in the same transaction as the order
            var orderCreatedEvent = new OrderCreated
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Items = order.Items.Select(item => new OrderItemEvent
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),
                ShippingAddress = order.ShippingAddress ?? "",
                BillingAddress = order.BillingAddress ?? "",
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            };

            // Publish BEFORE CommitAsync so outbox message is saved in the same transaction
            await _unitOfWork.OrderRepository.AddAsync(order, cancellationToken);
            await _publishEndpoint.Publish(orderCreatedEvent, cancellationToken);

            // Step 4: Commit — saves Order + OutboxMessage atomically
            var saveResult = await _unitOfWork.CommitAsync(cancellationToken);

            if (!saveResult)
            {
                _logger.LogError("Failed to save order to database for user {UserId}", request.UserId);
                return AppResult<OrderDto>.Failure("Failed to create order", 500);
            }

            _logger.LogInformation("Order {OrderId} saved and OrderCreated event queued in outbox, Saga will handle orchestration", order.Id);

            // Step 5: Map to DTO and return
            var orderDto = _mapper.Map<OrderDto>(order);
            _logger.LogInformation("Order {OrderId} created successfully for user {UserId}, waiting for Saga processing", 
                order.Id, request.UserId);
            return AppResult<OrderDto>.Success(orderDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating order for user {UserId}", request.UserId);
            return AppResult<OrderDto>.Failure(ex.Message, 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating order for user {UserId}", request.UserId);
            return AppResult<OrderDto>.Failure($"Error creating order: {ex.Message}", 500);
        }
    }
}
