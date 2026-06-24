using Contract.Order;
using MassTransit;
using MediatR;
using RecommendationService.Entities;
using RecommendationService.Persistence;
using RecommendationService.Services.UserActionTracking;

namespace RecommendationService.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly IMediator _mediator;
        private readonly IRecommendationUnitOfWork _unitOfWork;
        public OrderCreatedConsumer(IMediator mediator, IRecommendationUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var message = context.Message;
            foreach(var item in message.Items)
            {
                await _unitOfWork.UserActionTrackingRepository.AddAsync(new UserActionTracking
                {
                    UserId = message.UserId,
                    ActionType = UserActionType.Purchase,
                    ProductId = item.ProductId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _unitOfWork.CommitAsync();
        }
    }
}
