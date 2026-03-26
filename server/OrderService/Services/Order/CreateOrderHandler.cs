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

namespace OrderService.Services.Order;

/// <summary>
/// Handler for creating a new order with Saga orchestration
/// Uses event-driven pattern with Outbox for reliability
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, AppResult<OrderDto>>
{
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    // private readonly GrpcProduct.GrpcProductClient _productGrpcClient; // Commented: Using Saga 
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateBuilder _templateBuilder;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderUnitOfWork unitOfWork, 
        IMapper mapper,
        // GrpcProduct.GrpcProductClient productGrpcClient, // Commented: Using Saga
        IPublishEndpoint publishEndpoint,
        IEmailService emailService,
        IEmailTemplateBuilder templateBuilder,
        ILogger<CreateOrderHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        // _productGrpcClient = productGrpcClient; // Commented: Using Saga
        _publishEndpoint = publishEndpoint;
        _emailService = emailService;
        _templateBuilder = templateBuilder;
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
            var orderItems = dto.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                OrderId = string.Empty // Will be set after order is created
            }).ToList();

            var order = Entities.Order.CreateOrder(
                userId: request.UserId,
                items: orderItems,
                shippingAddress: dto.ShippingAddress,
                billingAddress: dto.BillingAddress,
                shippingCost: dto.ShippingCost,
                discount: dto.Discount
            );

            // Set order reference for items
            foreach (var item in order.Items)
            {
                item.OrderId = order.Id;
            }

            // Step 3: Save to database (with Outbox pattern enabled)
            await _unitOfWork.OrderRepository.AddAsync(order, cancellationToken);
            var saveResult = await _unitOfWork.CommitAsync(cancellationToken);

            if (!saveResult)
            {
                _logger.LogError("Failed to save order to database for user {UserId}", request.UserId);
                return AppResult<OrderDto>.Failure("Failed to create order", 500);
            }

            _logger.LogInformation("Order {OrderId} created successfully in database", order.Id);

            // Step 4: Publish OrderCreated event - Saga will orchestrate from here
            // Outbox pattern ensures event is delivered even if broker is down
            // Step 4: Publish OrderCreated event - Saga will orchestrate from here
            // Outbox pattern ensures event is delivered even if broker is down
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
                SubTotal = order.SubToTal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            };

            // Publish event - OrderSaga will pick this up and orchestrate stock reservation
            await _publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
            _logger.LogInformation("Published OrderCreated event for order {OrderId}, Saga will handle orchestration", order.Id);

            // Step 5: Map to DTO and return
            var orderDto = _mapper.Map<OrderDto>(order);
            _logger.LogInformation("Order {OrderId} created successfully for user {UserId}, waiting for Saga processing", 
                order.Id, request.UserId);
            var body = await _templateBuilder.BuildAsync("OrderConfirmation", new()
            {
                ["OrderId"] = order.Id.ToString(),
                ["CustomerName"] = order.UserId.ToString(), // Ideally should fetch user details for name
                ["TotalPrice"] = order.Total.ToString("C"),
                ["Address"] = order.BillingAddress ?? "N/A",
                ["OrderDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });

            await _emailService.SendEmailAsync("thaihoangnhantk17lqd@gmail.com", "Xác nhận đơn hàng", body);
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
