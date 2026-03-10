using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.EF.Application;

namespace Shared.Web.Controller
{
    [Route("[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()
            ?? throw new InvalidOperationException("Imediator service is unavailable");

        protected ActionResult HandleAppResult<T>(AppResult<T> result)
        {
            if (!result.IsSuccess && result.Code == 404) return NotFound(result.Error); // Trả về lỗi 404 nấu không tồn tại
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            return BadRequest(result.Error);
        }
    }
}