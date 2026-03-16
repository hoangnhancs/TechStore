using System;

namespace OrderService.Entities;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string UserId { get; set; }
    public required string UserID { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Created;
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public long SubToTal {get; set; }
    public long ShippingCost { get; set; } 
    public long Discount { get; set; } = 0;
    public long Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NotSelected;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public void UpdateOrder(List<OrderItem> items, string shippingAddress, string? billingAddress, PaymentMethod paymentMethod, long shippingCost, long discount)
    {
        Items = items;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        PaymentMethod = paymentMethod;
        SubToTal = items.Sum(x => x.UnitPrice * x.Quantity);
        Discount = discount;
        ShippingCost = shippingCost;
        Total = items.Sum(x => x.UnitPrice * x.Quantity) + shippingCost - discount;
        UpdatedAt = DateTime.UtcNow;
    } 
}

public enum PaymentStatus
{
    Pending = 0,         
    Paid = 1,   
}         
public enum OrderStatus
{
    Created,      
    Processing,       
    Shipped,         
    Delivered,      
    Completed,      
    Cancelled   
}
public enum PaymentMethod
{
    NotSelected,
    CashOnDelivery, 
    CreditCard,    
    VNpay,       
    Momo,       
    BankTransfer,  
}

