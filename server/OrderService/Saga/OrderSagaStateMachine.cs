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
    /// States:
    /// - WaitingForStockReservation: Wait for ProductService to reserve stock
    /// - WaitingForPayment: Payment intent created, waiting for user to complete payment (webhook)
    /// - Processing: Payment completed, confirming order
    /// - Completed: Order confirmed, stock committed
    /// - Cancelled: Order cancelled due to stock/payment failure
    /// </summary>
    public class OrderSagaStateMachine : MassTransitStateMachine<OrderSagaState>
    {
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
        public Event<ConfirmOrder>? ConfirmOrderEvent { get; set; }
        public Event<PaymentCompleted>? PaymentSucceededEvent { get; set; }
        public Event<PaymentFailed>? PaymentFailedEvent { get; set; }

        public OrderSagaStateMachine()
        {
            InstanceState(x => x.CurrentState);

            // Event correlation - convert string OrderId to Guid
            Event(() => OrderCreated, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId)
                .SelectId(context => Guid.Parse(context.Message.OrderId)));

            Event(() => StockReservedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => StockReservationFailedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => ConfirmOrderEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => PaymentSucceededEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            Event(() => PaymentFailedEvent, x => x.CorrelateBy(
                (state, context) => state.OrderId == context.Message.OrderId));

            // Initial state: Receive OrderCreated event
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

            During(WaitingForStockReservation,
                When(StockReservedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<CreatePayment>(new
                    {
                        UserId = ctx.Saga.UserId,
                        OrderId = ctx.Saga.OrderId,
                        Amount = ctx.Saga.Total,
                        Currency = ctx.Saga.Currency,
                        PaymentMethod = ctx.Saga.PaymentMethod
                    }))
                    .TransitionTo(WaitingForPayment),
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

            During(WaitingForPayment,
                When(PaymentSucceededEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId
                    }))
                    .TransitionTo(Processing),

                // Sad path: Payment failed
                When(PaymentFailedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.FailureReason = ctx.Message.ErrorMessage;
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<CancelOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Reason = ctx.Saga.FailureReason
                    }))
                    .TransitionTo(Cancelled)
            );

            During(Processing,
                When(ConfirmOrderEvent)
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
