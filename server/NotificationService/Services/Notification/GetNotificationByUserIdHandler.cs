using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        public GetNotificationByUserIdHandler(INotificationUnitOfWork unitOfWork, IMapper mapper, GrpcIdentityClient grpcIdentityClient )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _grpcIdentityClient = grpcIdentityClient;
        }
        public async Task<AppResult<List<UserNotificationDto>>> Handle(GetNotificationByUserIdQuery request, CancellationToken cancellationToken)
        {
            var notifications = (await _unitOfWork.NotificationRepository.GetListAsync(
                predicate: n => n.Recipients.Any(r => r.UserId == request.UserId),
                query: q => q.Include(n => n.Recipients),
                cancellationToken: cancellationToken
            )).OrderByDescending(n => n.CreatedAt);

            if (!notifications.Any())
            {
                return AppResult<List<UserNotificationDto>>.Success(new List<UserNotificationDto>()); // Return empty list if no notifications found
            }

            var allSenderIds = _grpcIdentityClient.GetUsersByIds(notifications.Select(n => n.SenderId ?? string.Empty).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList()).Result
                .ToDictionary(x => x.UserId, x => x);
            var systemUser = await _grpcIdentityClient.GetSystemUser();

            var notificationsDto = notifications.Select(n =>
            {
                var recipient = n.Recipients
                    .First(r => r.UserId == request.UserId);

                var sender = (string.IsNullOrEmpty(n.SenderId) || n.SenderId == systemUser?.UserId)
                    ? systemUser 
                    : (allSenderIds.ContainsKey(n.SenderId) ? allSenderIds[n.SenderId] : systemUser);

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
                    ReadAt = recipient.ReadAt
                };
            }).ToList();

            //var notificationDtos = _mapper.Map<List<UserNotificationsDto>>(notifications);
            return AppResult<List<UserNotificationDto>>.Success(notificationsDto);
        }
    }
}