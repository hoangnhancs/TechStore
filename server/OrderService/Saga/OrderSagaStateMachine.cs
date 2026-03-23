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
    /// Handles: Stock Reservation → (Future: Payment) → Order Confirmation
    /// With compensation for failures
    /// </summary>
    public class OrderSagaStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        // States
        public State? WaitingForStockReservation { get; set; }
        public State? StockReserved { get; set; }
        public State? Completed { get; set; }
        public State? Cancelled { get; set; }

        // Events
        public Event<OrderCreated>? OrderCreated { get; set; }
        public Event<StockReserved>? StockReservedEvent { get; set; }
        public Event<StockReservationFailed>? StockReservationFailedEvent { get; set; }

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
                    })
                    .PublishAsync(ctx => ctx.Init<ReserveStock>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        Items = ctx.Saga.Items
                    }))
                    .TransitionTo(WaitingForStockReservation)
            );

            // During stock reservation
            During(WaitingForStockReservation,
                // Happy path: Stock reserved successfully
                When(StockReservedEvent)
                    .Then(ctx =>
                    {
                        ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new
                    {
                        OrderId = ctx.Saga.OrderId
                    }))
                    .TransitionTo(Completed),

                // Sad path: Stock reservation failed
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

            // Mark saga as finalized when completed or cancelled
            SetCompletedWhenFinalized();
        }
    }
}