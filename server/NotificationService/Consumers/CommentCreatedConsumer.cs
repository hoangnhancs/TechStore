using Contract.Comment;
using MassTransit;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Services;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using static NotificationService.Entities.Notification;

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
            string? groupId = string.Empty;
            var parentCommentUserId = message.ParantCommentUserId;
            var isReply = !string.IsNullOrEmpty(message.ParentCommentId);

            List<string> ids = new List<string> { message.UserId };

            if (!string.IsNullOrEmpty(message.ParantCommentUserId)) 
                ids.Add(message.ParantCommentUserId);

            var users = await _identityClient.GetUsersByIds(ids);

            var user = users.Where(x => x.UserId == message.UserId).FirstOrDefault();
            var parentUser = users.Where(x => x.UserId == message.ParantCommentUserId).FirstOrDefault();

            if (user == null)
            {
                return;
            }

            var isSendToAdminGr = (isReply && (parentUser != null && parentUser.IsAdmin)) || !isReply; //isReply admin or new comment

            if (isSendToAdminGr) //if reply admin or new comment, will get groupId to send notification to all admin
            {
                groupId = (await _mediator.Send(new GetNotiGroupAllAdminQuery())).Value?.Id;
            }

            string messageContent = string.IsNullOrEmpty(parentCommentUserId) 
                ? $"{user.DisplayName} đã thêm bình luận mới:\n{message.Content}"
                : $"{user.DisplayName} đã trả lời bình luận của bạn:\n{message.Content}";

            await _mediator.Send(new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    /*người nhận sẽ là người được reply hoặc admin*/
                    ReceiverId = !isSendToAdminGr ? parentCommentUserId : null, 
                    GroupId = isSendToAdminGr ? groupId : null,
                    Title = "Bình luận mới",
                    Message = messageContent,
                    Link = message.Link,
                    Category = NotificationCategory.Interaction.ToString(),
                    Type = string.IsNullOrEmpty(parentCommentUserId) ? NotificationType.NewComment.ToString() : NotificationType.CommentReply.ToString(),
                    ReferenceId = message.CommentId,
                    ReferenceType = NotificationReferenceType.Comment.ToString(),
                    ParentReferenceId = message.ParentCommentId,
                    ParentReferenceType = string.IsNullOrEmpty(message.ParantCommentUserId) ? null : NotificationReferenceType.Comment.ToString(),
                    SenderId = message.UserId,
                    SenderName = user.UserName,
                    SenderDisplayName = user.DisplayName,
                    SenderImageUrl = user.ImageUrl
                }
            });
        }
    }
}
