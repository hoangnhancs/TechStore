using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.EF.Application;

namespace Shared.Web.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected ActionResult HandleAppResult<T>(AppResult<T> result)
        {
            return result switch
            {
                { IsSuccess: true } => Ok(result.Value),
                { Code: 401 }                        => Unauthorized(result.Error),
                { Code: 403 }                        => Forbid(),
                { Code: 404 }                        => NotFound(result.Error),
                _                                    => BadRequest(result.Error)
            };
        }
    }
}
