using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;

namespace OrderService.Saga
{
    /// <summary>
    /// Saga state to track order processing workflow
    /// </summary>
    public class OrderSagaState : SagaStateMachineInstance
    {
        /// <summary>
        /// Correlation ID - matches to OrderId (as Guid)
        /// </summary>
        public Guid CorrelationId { get; set; }
        
        /// <summary>
        /// Current state of the saga
        /// </summary>
        public string? CurrentState { get; set; }
        
        /// <summary>
        /// Order ID being processed
        /// </summary>
        public required string OrderId { get; set; }
        
        /// <summary>
        /// User who created the order
        /// </summary>
        public required string UserId { get; set; }
        
        /// <summary>
        /// Order items to reserve stock for
        /// </summary>
        public List<OrderItemEvent> Items { get; set; } = [];
        
        /// <summary>
        /// Total amount
        /// </summary>
        public long Total { get; set; }
        
        /// <summary>
        /// Timestamp when saga started
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Timestamp of last update
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Reason for failure (if any)
        /// </summary>
        public string? FailureReason { get; set; }
    }
}