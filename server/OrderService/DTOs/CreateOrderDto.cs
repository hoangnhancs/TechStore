namespace OrderService.DTOs;

/// <summary>
/// DTO for creating a new order
/// </summary>
public class CreateOrderDto
{
    public required string ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public required List<CreateOrderItemDto> Items { get; set; }
    public long ShippingCost { get; set; } = 0;
    public long Discount { get; set; } = 0;
}

/// <summary>
/// DTO for order items when creating an order
/// </summary>
public class CreateOrderItemDto
{
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }
    public required long UnitPrice { get; set; }
}
