using System;

namespace IdentityService.DTOs;

public class AddressDto
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? DetailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Type { get; set; }
    public bool IsDefault { get; set; }
}
