using Contract.Review;
using MassTransit;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Services;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using Org.BouncyCastle.Crypto;
using static NotificationService.Entities.Notification;

namespace NotificationService.Consumers
{
    public class ReviewCreatedConsumer : IConsumer<ReviewCreated>
    {
        private readonly IMediator _mediator;
        private readonly GrpcIdentityClient _identityClient;
        public ReviewCreatedConsumer(IMediator mediator, GrpcIdentityClient identityClient)
        {
            _mediator = mediator;
            _identityClient = identityClient;
        }
        public async Task Consume(ConsumeContext<ReviewCreated> context)
        {
            var message = context.Message;
           
            var groupId = (await _mediator.Send(new GetNotiGroupAllAdminQuery())).Value?.Id;
            var user = (await _identityClient.GetUsersByIds(new List<string> { message.UserId })).FirstOrDefault();

            string messageContent = $"{user?.DisplayName} đã đánh giá về sản phẩm:\n{message.Content}";

            await _mediator.Send(new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    /*người nhận sẽ là người được reply hoặc admin*/
                    ReceiverId = null,
                    GroupId = groupId,
                    Title = "Đánh giá mới",
                    Message = messageContent,
                    Category = NotificationCategory.Interaction.ToString(),
                    Type = NotificationType.NewReview.ToString(),
                    ReferenceId = message.ReviewId,
                    ReferenceType = NotificationReferenceType.Review.ToString(),
                    ParentReferenceId = null,
                    ParentReferenceType = null,
                    SenderId = message.UserId,
                    SenderName = user?.UserName,
                    SenderDisplayName = user?.DisplayName,
                    SenderImageUrl = user?.ImageUrl
                }
            });
        }
    }
}
