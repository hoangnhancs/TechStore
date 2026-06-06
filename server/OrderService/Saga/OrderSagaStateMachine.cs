using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Contract.Order;
using Contract.Payment;
using Contract.Product;
using MassTransit;
using Microsoft.Extensions.Options;
using OrderService.SettingConfig;
using static OrderService.Entities.Order;

namespace OrderService.Saga
{
    /// <summary>
    /// Saga orchestrator for order processing workflow
    /// Flow: Stock Reservation → Payment Creation → Payment Completion → Order Confirmation → Stock Commit
    ///
    /// States:
    /// - WaitingForStockReservation: Wait for ProductService to reserve stock
    /// - WaitingForConfirmation: COD orders waiting for admin confirmation
    /// - WaitingForPaymentCreated: Waiting for PaymentService to create payment intent
    /// - WaitingForPayment: Payment intent created, waiting for Stripe webhook (online only)
    /// - Processing: Payment completed / COD confirmed, confirming order
    /// - Completed: Order confirmed, stock committed
    /// - Cancelled: Order cancelled due to stock/payment failure or timeout
    ///
    /// Online Flow: Stock reserved → WaitingForPaymentCreated
    ///   → PaymentCreated → WaitingForPayment (Quartz timeout starts)
    ///   → PaymentCompleted → Processing → Completed
    ///   → PaymentFailed | Timeout → Cancelled + ReleaseStock
    ///
    /// COD Flow: Stock reserved → WaitingForConfirmation (admin confirms)
    ///   → WaitingForPaymentCreated → PaymentCreated → Processing → Completed
    ///   → PaymentFailed → back to WaitingForConfirmation (admin retries)
    /// </summary>
    public class OrderSagaStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        private readonly PaymentOptions _options;
        private readonly ILogger<OrderSagaStateMachine> _logger;

        public OrderSagaStateMachine(
            ILogger<OrderSagaStateMachine> logger,
            IOptions<PaymentOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            ConfigureSaga();
        }

        public record OrderOnlinePaymentExpired : OrderPaymentExpired; 
        public record OrderCodPaymentExpired : OrderPaymentExpired;

        // States
        public State? WaitingForStockReservation { get; set; }
        public State? WaitingForPaymentCreated { get; set; }
        public State? WaitingForPayment { get; set; }
        public State? WaitingForConfirmation { get; set; }
        public State? Processing { get; set; }
        public State? Completed { get; set; }
        public State? Cancelled { get; set; }

        // Events
        public Event<OrderCreated>? OrderCreated { get; set; }
        public Event<StockReserved>? StockReservedEvent { get; set; }
        public Event<StockReservationFailed>? StockReservationFailedEvent { get; set; }
        public Event<OrderConfirmed>? OrderConfirmedEvent { get; set; }
        public Event<PaymentCreated>? PaymentCreatedEvent { get; set; }
        public Event<PaymentCompleted>? PaymentSucceededEvent { get; set; }
        public Event<PaymentFailed>? PaymentFailedEvent { get; set; }
        public Event<ConfirmCodOrder>? ConfirmCodOrderEvent { get; set; }

        // Scheduled events
        public Schedule<OrderSagaState, OrderOnlinePaymentExpired>? OnlinePaymentExpirySchedule { get; set; }
        public Schedule<OrderSagaState, OrderCodPaymentExpired>? CodPaymentExpirySchedule { get; set; }


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

            Event(() => ConfirmCodOrderEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => PaymentCreatedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            // Schedule: auto-cancel if payment window expires
            Schedule(() => OnlinePaymentExpirySchedule, x => x.OnlinePaymentExpiryTokenId, s =>
            {
                s.Delay = TimeSpan.FromMinutes(_options.OnlinePaymentWindowMinutes);
                s.Received = r => r.CorrelateBy(
                    (state, ctx) => state.OrderId == ctx.Message.OrderId);
            });

            Schedule(() => CodPaymentExpirySchedule, x => x.CodPaymentExpiryTokenId, s =>
            {
                s.Delay = TimeSpan.FromMinutes(_options.CodPaymentWindowMinutes);
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
                        LoggerExtensions.LogInformation(this._logger, "[SAGA] StockReserved for OrderId: {OrderId}", (object)ctx.Saga.OrderId);
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .IfElse(
                        ctx => ctx.Saga.PaymentMethod == PaymentMethod.CashOnDelivery.ToString(),
                        // COD: put order in WaitingForConfirmation — requires manual admin confirm, no expiry timer. 
                        // Payment will be created only after admin confirms (to avoid ghost payments if user never confirms).
                        cod => cod
                            .PublishAsync(ctx => ctx.Init<SetOrderWaitingForConfirmation>(new
                            {
                                OrderId = ctx.Saga.OrderId
                            }))
                            .Schedule(CodPaymentExpirySchedule, ctx => ctx.Init<OrderCodPaymentExpired>(new
                            {
                                OrderId = ctx.Saga.OrderId,
                                UserId = ctx.Saga.UserId
                            })) //start expiry timer immediately for COD as well to prevent infinite waiting if user never confirms
                            .TransitionTo(WaitingForConfirmation),
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
                            // .Schedule(PaymentExpirySchedule, ctx => ctx.Init<OrderPaymentExpired>(new
                            // {
                            //     OrderId = ctx.Saga.OrderId,
                            //     UserId = ctx.Saga.UserId
                            // })) //ignore schedule for create payment row step, only start schedule when payment created successfully in WaitingForPaymentCreated state
                            .TransitionTo(WaitingForPaymentCreated)
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

            // ── WaitingForConfirmation (for COD) ────────────────────────────────────────
            During(WaitingForConfirmation,
                When(ConfirmCodOrderEvent)
                    .Then(ctx =>
                    {
                        _logger.LogInformation("[SAGA] COD Order Confirmed for OrderId: {OrderId}", ctx.Saga.OrderId);
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    //.PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                    //{
                    //    OrderId = ctx.Saga.OrderId
                    //}))
                    .PublishAsync(ctx => ctx.Init<CreatePayment>(new
                    {
                        UserId = ctx.Saga.UserId,
                        OrderId = ctx.Saga.OrderId,
                        Amount = ctx.Saga.Total,
                        Currency = ctx.Saga.Currency,
                        PaymentMethod = ctx.Saga.PaymentMethod
                    }))
                    .TransitionTo(WaitingForPaymentCreated),

                When(CodPaymentExpirySchedule!.Received)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = "Payment window expired — order auto-cancelled";
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        _logger.LogWarning("[SAGA] COD PaymentExpired for OrderId: {OrderId}", ctx.Saga.OrderId);
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

            // ── WaitingForPaymentCreated ────────────────────────────────────────────────
            During(WaitingForPaymentCreated,
                When(PaymentCreatedEvent)
                    .Then(ctx =>
                    {
                        _logger.LogInformation("[SAGA] PaymentCreated for OrderId: {OrderId}", ctx.Saga.OrderId);
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .IfElse(
                        ctx => ctx.Saga.PaymentMethod == PaymentMethod.CashOnDelivery.ToString(),
                        cod => cod
                            .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                            {
                                OrderId = ctx.Saga.OrderId
                            }))
                            .TransitionTo(Processing),
                        online => online
                            .Schedule(OnlinePaymentExpirySchedule, ctx => ctx.Init<OrderOnlinePaymentExpired>(new
                            {
                                OrderId = ctx.Saga.OrderId,
                                UserId = ctx.Saga.UserId
                            }))
                            .TransitionTo(WaitingForPayment)
                    ),
                When(PaymentFailedEvent)
                    .Then(ctx =>
                    {
                        _logger.LogWarning("[SAGA] Payment creation failed for OrderId: {OrderId}, Reason: {Reason}",
                            ctx.Saga.OrderId, ctx.Message.ErrorMessage);
                        ctx.Saga.FailureReason = ctx.Message.ErrorMessage;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<OrderNotification>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        UserId = ctx.Saga.UserId,
                        IsSuccess = false,
                        ErrorMessage = ctx.Saga.FailureReason,
                    }))
                    .IfElse(
                        ctx => ctx.Saga.PaymentMethod == PaymentMethod.CashOnDelivery.ToString(),
                        // COD: payment creation failed → notify admin, allow retry by re-confirming
                        cod => cod.TransitionTo(WaitingForConfirmation),
                        // Online: no retry — cancel immediately and release stock
                        online => online
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
                    )
            );



            // ── WaitingForPayment ────────────────────────────────────────────────
            During(WaitingForPayment,
                // Online payment succeeded
                When(PaymentSucceededEvent)
                    .Then(ctx =>
                    {
                        _logger.LogInformation("[SAGA] PaymentCompleted for OrderId: {OrderId}", ctx.Saga.OrderId);
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .Unschedule(OnlinePaymentExpirySchedule)
                    .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId
                    }))
                    .TransitionTo(Processing),

                // Online payment failed — cancel immediately, user returns to checkout to create new order
                When(PaymentFailedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = ctx.Message.ErrorMessage;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                        _logger.LogWarning("[SAGA] PaymentFailed for OrderId: {OrderId}, Reason: {Reason}",
                            ctx.Saga.OrderId, ctx.Saga.FailureReason);
                    })
                    .Unschedule(OnlinePaymentExpirySchedule)
                    .PublishAsync(ctx => ctx.Init<OrderNotification>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        UserId = ctx.Saga.UserId,
                        IsSuccess = false,
                        ErrorMessage = ctx.Saga.FailureReason,
                    }))
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
                    .TransitionTo(Cancelled),

                // Payment window expired → auto-cancel
                When(OnlinePaymentExpirySchedule!.Received) //PaymentExpirySchedule!.Received chinh la Event<Scheduled<OrderPaymentExpired>>
                    // .If(ctx => ctx.Saga.PaymentMethod != PaymentMethod.CashOnDelivery.ToString(), x => x
                        .Then(ctx =>
                        {
                            ctx.Saga.FailureReason = "Payment window expired — order auto-cancelled";
                            ctx.Saga.UpdatedAt = DateTime.UtcNow;
                            _logger.LogWarning("[SAGA] PaymentExpired for OrderId: {OrderId}", ctx.Saga.OrderId);
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
                    // )
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
                    .PublishAsync(ctx => ctx.Init<OrderNotification>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        UserId = ctx.Saga.UserId,
                        IsSuccess = true
                    }))
                    .TransitionTo(Completed)
            );

            // Mark saga as finalized when completed or cancelled
            SetCompletedWhenFinalized();
        }
    }
}

