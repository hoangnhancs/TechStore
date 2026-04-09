using System;

namespace CartService.DTOs;

public class BasketItemDto
{
    public int Id { get; set; } 
    public int Quantity { get; set; }
    //navigation properties
    public required string ProductId { get; set; }
    public required string BasketId { get; set; }
}
