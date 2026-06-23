using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using Stripe;
using static PaymentService.Entities.Payment;

namespace PaymentService.Jobs;

public class PaymentReconciliationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBus _bus;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentReconciliationJob> _logger;

    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(2);
    private const int MinAgeMinutes = 2;    // chờ webhook ít nhất 2 phút trước khi check
    private const int MaxAgeHours = 48;     // bỏ qua payment quá cũ (48h)

    public PaymentReconciliationJob(
        IServiceScopeFactory scopeFactory,
        IBus bus,
        IConfiguration config,
        ILogger<PaymentReconciliationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _bus = bus;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(RunInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Reconciliation] Unhandled error during reconciliation run");
            }
        }
    }

    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentSvcDbContext>();

        StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
        var stripeService = new PaymentIntentService();

        var now = DateTime.UtcNow;
        var minAge = now.AddMinutes(-MinAgeMinutes);
        var maxAge = now.AddHours(-MaxAgeHours);

        var pendingPayments = await db.Payments
            .Where(p =>
                p.PaymentMethod == PaymentMethodType.CreditCard &&
                p.Status == PaymentStatus.Pending &&
                p.PaymentIntentId != null &&
                p.CreatedAt <= minAge &&
                p.CreatedAt >= maxAge)
            .ToListAsync(cancellationToken);

        if (pendingPayments.Count == 0) return;

        _logger.LogInformation("[Reconciliation] Found {Count} pending CreditCard payment(s) to reconcile", pendingPayments.Count);

        foreach (var payment in pendingPayments)
        {
            try
            {
                var intent = await stripeService.GetAsync(payment.PaymentIntentId!, cancellationToken: cancellationToken);

                switch (intent.Status)
                {
                    case "succeeded":
                        payment.Status = PaymentStatus.Succeeded;
                        payment.UpdatedAt = now;
                        db.Payments.Update(payment);
                        await db.SaveChangesAsync(cancellationToken);

                        await _bus.Publish(new Contract.Payment.PaymentCompleted
                        {
                            OrderId = payment.OrderId
                        }, cancellationToken);

                        _logger.LogInformation("[Reconciliation] Recovered succeeded payment for OrderId: {OrderId}", payment.OrderId);
                        break;

                    case "canceled":
                    case "failed":
                        payment.Status = PaymentStatus.Failed;
                        payment.UpdatedAt = now;
                        db.Payments.Update(payment);
                        await db.SaveChangesAsync(cancellationToken);

                        await _bus.Publish(new Contract.Payment.PaymentFailed
                        {
                            OrderId = payment.OrderId,
                            ErrorMessage = $"Payment {intent.Status} — recovered by reconciliation job"
                        }, cancellationToken);

                        _logger.LogWarning("[Reconciliation] Recovered {Status} payment for OrderId: {OrderId}", intent.Status, payment.OrderId);
                        break;

                    default:
                        // requires_action, processing, requires_payment_method... bỏ qua, đợi lần sau
                        _logger.LogDebug("[Reconciliation] Skipping OrderId: {OrderId}, Stripe status: {Status}", payment.OrderId, intent.Status);
                        break;
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "[Reconciliation] Stripe API error for OrderId: {OrderId}", payment.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Reconciliation] Unexpected error for OrderId: {OrderId}", payment.OrderId);
            }
        }
    }
}
