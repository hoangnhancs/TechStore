using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using NotificationService.Services.Sender;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, AppResult<NotificationDto>>
    {
        private readonly INotificationUnitOfWork _notificationUnitOfWork;
        private readonly IMapper _mapper;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly INotificationServiceSender _notificationServiceSender;

        public CreateNotificationHandler(INotificationUnitOfWork notificationUnitOfWork, IMapper mapper, GrpcIdentityClient grpcIdentityClient, INotificationServiceSender notificationServiceSender)
        {
            _notificationUnitOfWork = notificationUnitOfWork;
            _mapper = mapper;
            _grpcIdentityClient = grpcIdentityClient;
            _notificationServiceSender = notificationServiceSender;
        }

        public async Task<AppResult<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var noti = _mapper.Map<Entities.Notification>(request.CreateNotificationDto);

            if (string.IsNullOrEmpty(request.CreateNotificationDto.ReceiverId) && string.IsNullOrEmpty(request.CreateNotificationDto.GroupId))
                return AppResult<NotificationDto>.Failure("ReceiverId or GroupId must be provided", 400);

            var receivers = new List<string>();
            if (!string.IsNullOrEmpty(request.CreateNotificationDto.GroupId))
            {
                var group = await _notificationUnitOfWork.NotificationGroupRepository
                    .GetByIdWithMembersAsync(request.CreateNotificationDto.GroupId, cancellationToken);

                if (group?.Members != null)
                    receivers.AddRange(group.Members.Select(m => m.UserId));
            }
            else
            {
                receivers.Add(request.CreateNotificationDto.ReceiverId
                    ?? throw new InvalidOperationException("ReceiverId must be provided when GroupId is not specified"));
            }

            foreach (var receiverId in receivers)
                noti.AddRecipient(receiverId);

            await _notificationUnitOfWork.NotificationRepository.AddAsync(noti);
            var res = await _notificationUnitOfWork.CommitAsync();

            if (!res) return AppResult<NotificationDto>.Failure("Failed to create notification", 500);

            var notificationDto = _mapper.Map<NotificationDto>(noti);
            var senderInfo = (await _grpcIdentityClient.GetUsersByIds([request.CreateNotificationDto.SenderId])).FirstOrDefault();
            notificationDto.SenderId = senderInfo?.UserId ?? request.CreateNotificationDto.SenderId;
            notificationDto.SenderName = senderInfo?.UserName ?? request.CreateNotificationDto.SenderName;
            notificationDto.SenderDisplayName = senderInfo?.DisplayName ?? request.CreateNotificationDto.SenderDisplayName;
            notificationDto.CreatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.CreateNotificationDto.GroupId))
            {
                var adminGroup = await _notificationUnitOfWork.NotificationGroupRepository
                    .GetByNameAsync(NotificationGroups.AllAdminsNotiGroupName, cancellationToken);

                if (adminGroup != null)
                    await _notificationServiceSender.SendToGroupAsync(adminGroup.Name, notificationDto);
            }
            else if (!string.IsNullOrEmpty(request.CreateNotificationDto.ReceiverId))
            {
                await _notificationServiceSender.SendToUserAsync(request.CreateNotificationDto.ReceiverId, notificationDto);
            }

            return AppResult<NotificationDto>.Success(notificationDto);
        }
    }
}
