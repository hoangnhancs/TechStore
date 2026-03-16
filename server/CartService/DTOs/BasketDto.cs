using System;

namespace CartService.DTOs;

public class BasketDto
{
    public string Id { get; set; } = null!;
    public required string UserId { get; set; }
    public List<BasketItemDto> Items { get; set; } = [];
}
