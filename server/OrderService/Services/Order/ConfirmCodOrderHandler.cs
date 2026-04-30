using MassTransit;
using MediatR;
using Shared.Core.EF.Application;

namespace OrderService.Services.Order;

public class ConfirmCodOrderCommand : IRequest<AppResult<Unit>>
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public bool IsAdmin { get; set; }
}

public class ConfirmCodOrderHandler : IRequestHandler<ConfirmCodOrderCommand, AppResult<Unit>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ConfirmCodOrderHandler> _logger;

    public ConfirmCodOrderHandler(IPublishEndpoint publishEndpoint, ILogger<ConfirmCodOrderHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AppResult<Unit>> Handle(ConfirmCodOrderCommand request, CancellationToken cancellationToken)
    {
        // Only admin can confirm COD orders (prevents spam/fake orders)
        if (!request.IsAdmin)
            return AppResult<Unit>.Failure("Only admins can confirm COD orders", 403);

        await _publishEndpoint.Publish(new Contract.ConfirmCodOrder
        {
            OrderId = request.OrderId
        }, cancellationToken);

        _logger.LogInformation("ConfirmCodOrder published for OrderId: {OrderId} by admin", request.OrderId);
        return AppResult<Unit>.Success(Unit.Value);
    }
}
