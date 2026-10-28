using Grpc.Core;
using MediatR;
using VoucherService.Grpc;
using VoucherService.Services.Voucher;

namespace VoucherService.Services.Grpc;

/// <summary>
/// gRPC server — only OrderService calls this (sync, not exposed through Gateway,
/// same pattern as ProductService's gRPC ReserveStock — plan mục A.2/D).
/// </summary>
public class GrpcVoucherService : GrpcVoucher.GrpcVoucherBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<GrpcVoucherService> _logger;

    public GrpcVoucherService(IMediator mediator, ILogger<GrpcVoucherService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<ApplyVoucherResponse> ApplyVoucher(ApplyVoucherRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received ApplyVoucher request for Code {Code}, OrderId {OrderId}", request.Code, request.OrderId);

        var items = request.Items.Select(i => new VoucherDiscountCalculator.CalcItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList();

        var result = await _mediator.Send(new ApplyVoucherCommand
        {
            Code = request.Code,
            OrderId = request.OrderId,
            UserId = request.UserId,
            SubTotal = request.SubTotal,
            ShippingCost = request.ShippingCost,
            Items = items
        });

        if (!result.IsSuccess || result.Value == null)
        {
            return new ApplyVoucherResponse
            {
                Success = false,
                ErrorMessage = result.Error ?? "Failed to apply voucher"
            };
        }

        return new ApplyVoucherResponse
        {
            Success = true,
            DiscountAmount = result.Value.DiscountAmount,
            VoucherId = result.Value.VoucherId,
            VoucherCode = result.Value.VoucherCode
        };
    }

    public override async Task<ReleaseVoucherResponse> ReleaseVoucher(ReleaseVoucherRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received ReleaseVoucher request for OrderId {OrderId}", request.OrderId);

        var result = await _mediator.Send(new ReleaseVoucherCommand { OrderId = request.OrderId });

        return new ReleaseVoucherResponse
        {
            Success = result.IsSuccess,
            ErrorMessage = result.IsSuccess ? string.Empty : (result.Error ?? "Failed to release voucher")
        };
    }
}
