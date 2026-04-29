using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using Shared.Core.EF.Application;
using static OrderService.Entities.Order;

namespace OrderService.Services.Order;

public class RetryPaymentCommand : IRequest<AppResult<Unit>>
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public required string PaymentMethod { get; set; }
}

public class RetryPaymentHandler : IRequestHandler<RetryPaymentCommand, AppResult<Unit>>
{
    private readonly OrderSvcDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RetryPaymentHandler> _logger;

    public RetryPaymentHandler(OrderSvcDbContext context, IPublishEndpoint publishEndpoint, ILogger<RetryPaymentHandler> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AppResult<Unit>> Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order == null)
            return AppResult<Unit>.Failure("Order not found", 404);

        if (order.UserId != request.UserId)
            return AppResult<Unit>.Failure("You do not own this order", 403);

        if (order.Status != OrderStatus.WaitingForPayment)
            return AppResult<Unit>.Failure($"Cannot retry payment for order in status {order.Status}", 400);

        if (order.PmtMethod == PaymentMethod.CashOnDelivery)
            return AppResult<Unit>.Failure("COD orders do not require online payment retry", 400);

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var newMethod))
            return AppResult<Unit>.Failure($"Invalid payment method: {request.PaymentMethod}", 400);

        // Publish to Saga — Saga will re-schedule expiry and call CreatePayment
        await _publishEndpoint.Publish(new Contract.RetryPayment
        {
            OrderId = request.OrderId,
            UserId = request.UserId,
            PaymentMethod = request.PaymentMethod,
            Currency = "VND",
            Amount = order.Total
        }, cancellationToken);

        _logger.LogInformation("RetryPayment published for OrderId: {OrderId}", request.OrderId);
        return AppResult<Unit>.Success(Unit.Value);
    }
}
