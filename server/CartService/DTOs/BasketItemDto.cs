using System;

namespace CartService.DTOs;

public class BasketItemDto
{
    public int Id { get; set; } 
    public int Quantity { get; set; }
    //navigation properties
    public required string ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryDisplayName { get; set; }
    public required string BasketId { get; set; }
}
