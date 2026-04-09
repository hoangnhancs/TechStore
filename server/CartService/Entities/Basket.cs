using System;
using Shared.Core.EF.Domain.Entities;

namespace CartService.Entities;

public class Basket : BaseEntity<string>
{
    public required string UserId { get; set; }
    public List<BasketItem> Items { get; set; } = [];
    public Basket() : base(Guid.NewGuid().ToString())
    {
    }
    public void AddItem(string productId, int quantity)
    {
        if (string.IsNullOrEmpty(productId)) ArgumentNullException.ThrowIfNull(productId);
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));

        var existingItem = FindItem(productId);
        if (existingItem != null) existingItem.Quantity += quantity;
        else
        {
            Items.Add(new BasketItem
            {   
                ProductId = productId,
                Quantity = quantity,
                BasketId = Id
            });
        }
    }

    public void RemoveItem(string productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));

        var item = FindItem(productId);

        if (item == null) return;

        if (quantity >= item.Quantity)
        {
            Items.Remove(item);
        }
        else
        {
            item.Quantity -= quantity;
        }
    }

    private BasketItem? FindItem(string productId)
    {
        return Items.FirstOrDefault(x => x.ProductId == productId);
    }
    public void RemovePermanentItem(string productId)
    {
        var item = FindItem(productId);
        if (item != null) Items.Remove(item);
    } 
}
