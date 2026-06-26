using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class GetNotificationByUserIdHandler : IRequestHandler<GetNotificationByUserIdQuery, AppResult<List<UserNotificationDto>>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GrpcIdentityClient _grpcIdentityClient;

        public GetNotificationByUserIdHandler(INotificationUnitOfWork unitOfWork, IMapper mapper, GrpcIdentityClient grpcIdentityClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _grpcIdentityClient = grpcIdentityClient;
        }

        public async Task<AppResult<List<UserNotificationDto>>> Handle(GetNotificationByUserIdQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _unitOfWork.NotificationRepository
                .GetByUserIdWithRecipientsAsync(request.UserId, cancellationToken);

            if (!notifications.Any())
                return AppResult<List<UserNotificationDto>>.Success([]);

            var senderIds = notifications
                .Where(n => !string.IsNullOrEmpty(n.SenderId))
                .Select(n => n.SenderId!)
                .Distinct()
                .ToList();

            var allSenderIds = (await _grpcIdentityClient.GetUsersByIds(senderIds))
                .ToDictionary(x => x.UserId, x => x);
            var systemUser = await _grpcIdentityClient.GetSystemUser();

            var notificationsDto = notifications.Select(n =>
            {
                var recipient = n.Recipients.First(r => r.UserId == request.UserId);

                var sender = (string.IsNullOrEmpty(n.SenderId) || n.SenderId == systemUser?.UserId)
                    ? systemUser
                    : (allSenderIds.TryGetValue(n.SenderId, out var s) ? s : systemUser);

                return new UserNotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Link = n.Link,
                    Category = n.Category.ToString(),
                    ReferenceId = n.ReferenceId,
                    ReferenceType = n.ReferenceType.ToString(),
                    ParentReferenceId = n.ParentReferenceId,
                    ParentReferenceType = n.ParentReferenceType.ToString(),
                    SenderId = n.SenderId ?? string.Empty,
                    SenderName = sender?.UserName ?? "System",
                    SenderDisplayName = sender?.DisplayName ?? "System",
                    SenderImageUrl = sender?.ImageUrl,
                    Type = n.Type.ToString(),
                    IsRead = recipient.IsRead,
                    ReadAt = recipient.ReadAt,
                    SentAt = recipient.SentAt,
                    CreatedAt = n.CreatedAt
                };
            }).ToList();

            return AppResult<List<UserNotificationDto>>.Success(notificationsDto);
        }
    }
}
