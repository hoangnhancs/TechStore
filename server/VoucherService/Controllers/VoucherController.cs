using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Controller;
using VoucherService.DTOs;
using VoucherService.Services.Voucher;

namespace VoucherService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoucherController : BaseApiController
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
    {
        return HandleAppResult(await Mediator.Send(new CreateVoucherCommand { Dto = dto }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return HandleAppResult(await Mediator.Send(new GetListVouchersQuery { PageNumber = pageNumber, PageSize = pageSize }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        return HandleAppResult(await Mediator.Send(new GetVoucherByIdQuery { Id = id }));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateVoucherDto dto)
    {
        return HandleAppResult(await Mediator.Send(new UpdateVoucherCommand { Id = id, Dto = dto }));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        return HandleAppResult(await Mediator.Send(new DeleteVoucherCommand { Id = id }));
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateVoucherDto dto)
    {
        return HandleAppResult(await Mediator.Send(new ValidateVoucherQuery { Dto = dto }));
    }
}
