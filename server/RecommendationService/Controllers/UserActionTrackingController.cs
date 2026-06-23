using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecommendationService.DTOs;
using RecommendationService.Services.UserActionTracking;
using Shared.Web.Controller;
using System.Security.Claims;

namespace RecommendationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActionTrackingController : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> CreateUserActionTracking([FromBody] CreateUserActionTrackingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return HandleAppResult(await Mediator.Send(new CreateUserActionTrackingCommand
            {
                UserId = userId,
                ProductId = dto.ProductId,
                ActionType = dto.ActionType
            }));
        }
    }
}
