using System;

namespace OrderService.DTOs;

public class OrderDto
{
    public string Id { get; set; } = null!;
    public string? UserId { get; set; }
    public string? RecipientName { get; set; }
    public string? UserEmail { get; set; }
    public required string RecipientPhone { get; set; }
    public required string OrderNo { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public string Status { get; set; } = null!;
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public long SubToTal { get; set; }
    public long ShippingCost { get; set; }
    public long Discount { get; set; }
    public long Total { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ShipmentDto? Shipment { get; set; }
    public string PmtMethod { get; set; } = null!;
    public string PmtStatus { get; set; } = null!;
    public List<OrderStatusHistoryDto> StatusHistories { get; set; } = [];
}
