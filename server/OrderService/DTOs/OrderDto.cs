using System;

namespace OrderService.DTOs;

public class OrderDto
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public long SubToTal { get; set; }
    public long ShippingCost { get; set; }
    public long Discount { get; set; }
    public long Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ShipmentDto? Shipment { get; set; }
    public List<OrderStatusHistoryDto> StatusHistories { get; set; } = [];
}
