using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;

namespace OrderService.Saga
{
    /// <summary>
    /// Saga orchestrator for order processing workflow
    /// Flow: Stock Reservation → Payment Creation → Payment Completion → Order Confirmation → Stock Commit
    /// 
    /// States:
    /// - WaitingForStockReservation: Wait for ProductService to reserve stock
    /// - WaitingForPayment: Payment intent created, waiting for user to complete payment
    ///   Also used for COD orders waiting for manual confirmation
    /// - Processing: Payment completed / COD confirmed, confirming order
    /// - Completed: Order confirmed, stock committed
    /// - Cancelled: Order cancelled due to stock/payment failure or timeout
    ///
    /// Payment Retry: When PaymentFailed, saga stays in WaitingForPayment (up to MaxRetries)
    ///   and publishes OrderPaymentFailed so FE can let user retry or change method.
    /// Auto-cancel: A scheduled message (OrderPaymentExpired) is sent after PaymentWindowMinutes.
    ///   It fires if user never pays. Re-scheduled on each retry.
    /// COD: Stock reserved → order goes to WaitingForPayment. User/admin confirms manually
    ///   via ConfirmCodOrder command → order moves to Processing.
    /// </summary>
    public class OrderSagaStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        private const int MaxPaymentRetries = 3;
        private const int PaymentWindowMinutes = 1;

        // States
        public State? WaitingForStockReservation { get; set; }
        public State? WaitingForPayment { get; set; }
        public State? Processing { get; set; }
        public State? Completed { get; set; }
        public State? Cancelled { get; set; }

        // Events
        public Event<OrderCreated>? OrderCreated { get; set; }
        public Event<StockReserved>? StockReservedEvent { get; set; }
        public Event<StockReservationFailed>? StockReservationFailedEvent { get; set; }
        public Event<OrderConfirmed>? OrderConfirmedEvent { get; set; }
        public Event<PaymentCompleted>? PaymentSucceededEvent { get; set; }
        public Event<PaymentFailed>? PaymentFailedEvent { get; set; }
        public Event<RetryPayment>? RetryPaymentEvent { get; set; }
        public Event<ConfirmCodOrder>? ConfirmCodOrderEvent { get; set; }

        // Scheduled events
        public Schedule<OrderSagaState, OrderPaymentExpired>? PaymentExpirySchedule { get; set; }

        public ILogger<OrderSagaStateMachine> Logger { get; }

        public OrderSagaStateMachine(ILogger<OrderSagaStateMachine> logger)
        {
            Logger = logger;
            ConfigureSaga();
        }

        private void ConfigureSaga()
        {
            InstanceState(x => x.CurrentState);

            // Event correlation
            Event(() => OrderCreated, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId)
                .SelectId(context => Guid.Parse(context.Message.OrderId)));

            Event(() => StockReservedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => StockReservationFailedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => OrderConfirmedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => PaymentSucceededEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => PaymentFailedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => RetryPaymentEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => ConfirmCodOrderEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            // Schedule: auto-cancel if payment window expires
            Schedule(() => PaymentExpirySchedule, x => x.PaymentExpiryTokenId, s =>
            {
                s.Delay = TimeSpan.FromMinutes(PaymentWindowMinutes);
                s.Received = r => r.CorrelateBy(
                    (state, ctx) => state.OrderId == ctx.Message.OrderId);
            });

            // ── Initial ──────────────────────────────────────────────────────────
            Initially(
                When(OrderCreated)
                    .Then(ctx =>
                    {
                        ctx.Saga.OrderId = ctx.Message.OrderId;
                        ctx.Saga.UserId = ctx.Message.UserId;
                        ctx.Saga.Items = ctx.Message.Items;
                        ctx.Saga.Total = ctx.Message.Total;
                        ctx.Saga.CreatedAt = ctx.Message.CreatedAt;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        ctx.Saga.Currency = ctx.Message.Currency;
                        ctx.Saga.PaymentMethod = ctx.Message.PaymentMethod;
                        ctx.Saga.PaymentRetryCount = 0;
                    })
                    .PublishAsync(ctx => ctx.Init<ReserveStock>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Items = ctx.Saga.Items
                    }))
                    .TransitionTo(WaitingForStockReservation)
            );

            // ── WaitingForStockReservation ────────────────────────────────────────
            During(WaitingForStockReservation,
                When(StockReservedEvent)
                    .Then(ctx =>
                    {
                        Logger.LogInformation("[SAGA] StockReserved for OrderId: {OrderId}", ctx.Saga.OrderId);
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .IfElse(
                        ctx => ctx.Saga.PaymentMethod == "CashOnDelivery",
                        // COD: put order in WaitingForPayment — requires manual admin confirm, no expiry timer
                        cod => cod
                            .PublishAsync(ctx => ctx.Init<SetOrderWaitingForPayment>(new
                            {
                                OrderId = ctx.Saga.OrderId
                            }))
                            .TransitionTo(WaitingForPayment),
                        // Online payment: create payment intent + start expiry countdown
                        online => online
                            .PublishAsync(ctx => ctx.Init<CreatePayment>(new
                            {
                                UserId = ctx.Saga.UserId,
                                OrderId = ctx.Saga.OrderId,
                                Amount = ctx.Saga.Total,
                                Currency = ctx.Saga.Currency,
                                PaymentMethod = ctx.Saga.PaymentMethod
                            }))
                            .Schedule(PaymentExpirySchedule, ctx => ctx.Init<OrderPaymentExpired>(new
                            {
                                OrderId = ctx.Saga.OrderId,
                                UserId = ctx.Saga.UserId
                            }))
                            .TransitionTo(WaitingForPayment)
                    ),
                When(StockReservationFailedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = ctx.Message.Reason;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Reason = ctx.Saga.FailureReason
                    }))
                    .TransitionTo(Cancelled)
            );

            // ── WaitingForPayment ────────────────────────────────────────────────
            During(WaitingForPayment,
                // Online payment succeeded
                When(PaymentSucceededEvent)
                    .Then(ctx =>
                    {
                        Logger.LogInformation("[SAGA] PaymentCompleted for OrderId: {OrderId}", ctx.Saga.OrderId);
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .Unschedule(PaymentExpirySchedule)
                    .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId
                    }))
                    .TransitionTo(Processing),

                // Online payment failed — allow retry up to MaxRetries
                When(PaymentFailedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = ctx.Message.ErrorMessage;
                        ctx.Saga.PaymentRetryCount++;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        Logger.LogWarning("[SAGA] PaymentFailed for OrderId: {OrderId}, attempt {Count}", ctx.Saga.OrderId, ctx.Saga.PaymentRetryCount);
                    })
                    .IfElse(
                        ctx => ctx.Saga.PaymentRetryCount >= MaxPaymentRetries,
                        // Too many retries → cancel
                        tooMany => tooMany
                            .Unschedule(PaymentExpirySchedule)
                            .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                            {
                                OrderId = ctx.Saga.OrderId,
                                Reason = $"Payment failed after {MaxPaymentRetries} attempts: {ctx.Saga.FailureReason}"
                            }))
                            .TransitionTo(Cancelled),
                        // Still within retries → notify FE, stay in WaitingForPayment
                        canRetry => canRetry
                            .PublishAsync(ctx => ctx.Init<OrderPaymentFailed>(new
                            {
                                OrderId = ctx.Saga.OrderId,
                                UserId = ctx.Saga.UserId,
                                ErrorMessage = ctx.Saga.FailureReason,
                                RetryCount = ctx.Saga.PaymentRetryCount
                            }))
                        // Stays in WaitingForPayment — no TransitionTo
                    ),

                // User retries with same or different payment method
                When(RetryPaymentEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.PaymentMethod = ctx.Message.PaymentMethod;
                        ctx.Saga.Currency = ctx.Message.Currency;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        Logger.LogInformation("[SAGA] RetryPayment for OrderId: {OrderId}", ctx.Saga.OrderId);
                    })
                    // Reset expiry window on retry
                    .Unschedule(PaymentExpirySchedule)
                    .Schedule(PaymentExpirySchedule, ctx => ctx.Init<OrderPaymentExpired>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        UserId = ctx.Saga.UserId
                    }))
                    .PublishAsync(ctx => ctx.Init<CreatePayment>(new
                    {
                        UserId = ctx.Saga.UserId,
                        OrderId = ctx.Saga.OrderId,
                        Amount = ctx.Saga.Total,
                        Currency = ctx.Saga.Currency,
                        PaymentMethod = ctx.Saga.PaymentMethod
                    })),
                // COD manual confirm by admin
                When(ConfirmCodOrderEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        Logger.LogInformation("[SAGA] ConfirmCodOrder for OrderId: {OrderId}", ctx.Saga.OrderId);
                    })
                    .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId
                    }))
                    .TransitionTo(Processing),

                // Payment window expired → auto-cancel
                When(PaymentExpirySchedule!.Received)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = "Payment window expired — order auto-cancelled";
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        Logger.LogWarning("[SAGA] PaymentExpired for OrderId: {OrderId}", ctx.Saga.OrderId);
                    })
                    .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Reason = ctx.Saga.FailureReason
                    }))
                    .PublishAsync(ctx => ctx.Init<ReleaseStock>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Items = ctx.Saga.Items
                    }))
                    .TransitionTo(Cancelled)
            );

            // ── Processing ───────────────────────────────────────────────────────
            During(Processing,
                When(OrderConfirmedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<CommitStock>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Items = ctx.Saga.Items
                    }))
                    .TransitionTo(Completed)
            );

            // Mark saga as finalized when completed or cancelled
            SetCompletedWhenFinalized();
        }
    }
}

