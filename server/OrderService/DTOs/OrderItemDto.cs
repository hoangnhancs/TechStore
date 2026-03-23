using System;

namespace OrderService.DTOs;

public class OrderItemDto
{
    public required string ProductId { get; set; } 
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public string? OrderId { get; set; } 
}
