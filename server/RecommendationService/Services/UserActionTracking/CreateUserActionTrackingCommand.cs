using MediatR;
using Shared.Core.EF.Application;

namespace RecommendationService.Services.UserActionTracking
{
    public class CreateUserActionTrackingCommand : IRequest<AppResult<int>>
    {
        public string? UserId { get; set; }
        public required string ProductId { get; set; }
        public required string ActionType { get; set; }
    }
}
