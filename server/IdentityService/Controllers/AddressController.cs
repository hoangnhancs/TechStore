using System;
using System.Security.Claims;
using IdentityService.DTOs;
using IdentityService.Services.Address;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Controller;
using static IdentityService.Services.Address.CreateVirtualAddresses;

namespace IdentityService.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AddressController(IConfiguration configuration) : BaseApiController
{
    // [HttpPost("create-address")]
    // [Authorize(Policy = "SecurityStampRequirement")]
    // public async Task<IActionResult> CreateAddress([FromBody] AddressDto dto)
    // {
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (string.IsNullOrEmpty(userId))
    //         return Unauthorized("User not authenticated");
    //     return HandleAppResult(await Mediator.Send(new AddAddressCommand { UserId = userId, Address = dto }));
    // }

    // [HttpPut("update-address/{id}")]
    // [Authorize(Policy = "SecurityStampRequirement")]
    // public async Task<IActionResult> UpdateAddress(string id, [FromBody] AddressDto address)
    // {
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (string.IsNullOrEmpty(userId))
    //         return Unauthorized("User not authenticated");
    //     return HandleAppResult(await Mediator.Send(new UpdateAddressCommand { UserId = userId, AddressId = id, Address = address }));
    // }

    // [HttpGet("myaddresses")]
    // [Authorize(Policy = "SecurityStampRequirement")]
    // public async Task<IActionResult> GetAddresses()
    // {
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (string.IsNullOrEmpty(userId))
    //         return Unauthorized("User not authenticated");
    //     return HandleAppResult(await Mediator.Send(new GetAddressesByUserIdQuery { UserId = userId }));
    // }

    // [HttpGet("myaddresses/{id}")]
    // [Authorize(Policy = "SecurityStampRequirement")]
    // public async Task<IActionResult> GetAddressById(string id)
    // {
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (string.IsNullOrEmpty(userId))
    //         return Unauthorized("User not authenticated");
    //     return HandleAppResult(await Mediator.Send(new GetAddressByIdQuery { AddressId = id }));
    // }

    // [HttpDelete("delete-address")]
    // [Authorize(Policy = "SecurityStampRequirement")]
    // public async Task<IActionResult> DeleteAddress([FromBody] DeleteAddressDto dto)
    // {
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (string.IsNullOrEmpty(userId))
    //         return Unauthorized("User not authenticated");
    //     return HandleAppResult(await Mediator.Send(new DeleteAddressCommand { AddressId = dto.AddressId, UserId = userId }));
    // }


    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces()
    {
        var token = configuration["GHN:ApiToken"];
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Token", token);
        var response = await client.GetAsync("https://online-gateway.ghn.vn/shiip/public-api/master-data/province");
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Failed to fetch provinces");

        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] string provinceId)
    {
        var token = configuration["GHN:ApiToken"];
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Token", token);
        var response = await client.GetAsync($"https://online-gateway.ghn.vn/shiip/public-api/master-data/district?province_id={provinceId}");
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Failed to fetch districts");

        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }

    [HttpGet("wards")]
    public async Task<IActionResult> GetWards([FromQuery] string districtId)
    {
        var token = configuration["GHN:ApiToken"];
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Token", token);
        var response = await client.GetAsync($"https://online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={districtId}");
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Failed to fetch wards");

        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }
    
    [HttpPost("create-virtual-addresses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateVirtualAddresses()
    {
        return HandleAppResult(await Mediator.Send(new CreateVirtualAddressesCommand()));
    }
}
