using System;
using Shared.Core.EF.Domain.Entities;

namespace OrderService.Entities;

public class OrderItem : BaseEntity<int>
{
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public required string ProductId { get; set; }
    public required string OrderId { get; set; }
    public Order? Order { get; set; }
}
