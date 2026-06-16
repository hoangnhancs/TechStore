using Contract.Comment;
using MassTransit;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Services;
using NotificationService.Services.Notification;

namespace NotificationService.Consumers
{
    public class CommentCreatedConsumer : IConsumer<CommentCreated>
    {
        private readonly IMediator _mediator;
        private readonly GrpcIdentityClient _identityClient;
        public CommentCreatedConsumer(IMediator mediator, GrpcIdentityClient identityClient)
        {
            _mediator = mediator;
            _identityClient = identityClient;
        }
        public async Task Consume(ConsumeContext<CommentCreated> context)
        {
            var message = context.Message;
            string groupId = string.Empty;
            var parentCommentUserId = message.ParantCommentUserId;
            var user = (await _identityClient.GetUsersByIds(new List<string> { message.UserId })).FirstOrDefault();
  
            _mediator.Send(new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {

                }
            });
        }
    }
}
