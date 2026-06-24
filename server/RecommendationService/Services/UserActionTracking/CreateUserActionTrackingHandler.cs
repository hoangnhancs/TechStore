using MediatR;
using RecommendationService.Entities;
using RecommendationService.Persistence;
using RecommendationService.Repositories;
using RecommendationService.Repositories.Interface;
using Shared.Core.EF.Application;
using Shared.Core.EF.UnitOfWork;

namespace RecommendationService.Services.UserActionTracking
{
    public class CreateUserActionTrackingHandler : IRequestHandler<CreateUserActionTrackingCommand, AppResult<int>>
    {
        private readonly IRecommendationUnitOfWork _unitOfWork;
        public CreateUserActionTrackingHandler(IRecommendationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
       
        public async Task<AppResult<int>> Handle(CreateUserActionTrackingCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == null) return AppResult<int>.Success(0);
            var newTracking = new Entities.UserActionTracking
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                ActionType = Enum.Parse<UserActionType>(request.ActionType)
            };
            await _unitOfWork.UserActionTrackingRepository.AddAsync(newTracking, cancellationToken);
            var result = await _unitOfWork.CommitAsync(cancellationToken);
            if (!result) return AppResult<int>.Failure("Problem when create user action tracking", 400);
            return AppResult<int>.Success(newTracking.Id);
        }
    }
}
