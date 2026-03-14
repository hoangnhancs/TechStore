using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;



public class BasketItem
{
    public int Id { get; set; } 
    public int Quantity { get; set; }
    //navigation properties
    public required string ProductId { get; set; }
    public required string BasketId { get; set; }
    public Basket Basket { get; set; } = null!;
}