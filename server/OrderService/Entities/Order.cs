using System;
using Shared.Core.EF.Domain.Entities;

namespace OrderService.Entities;

public class Order : BaseEntity<string>
{
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

    public Order() : base(Guid.NewGuid().ToString())
    {
    }

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

