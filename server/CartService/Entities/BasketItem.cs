using System.ComponentModel.DataAnnotations.Schema;
using Shared.Core.EF.Domain.Entities;

namespace CartService.Entities;



public class BasketItem : BaseEntity<int>
{
    public int Quantity { get; set; }
    //navigation properties
    public required string ProductId { get; set; }
    public required string BasketId { get; set; }
    public Basket Basket { get; set; } = null!;
}