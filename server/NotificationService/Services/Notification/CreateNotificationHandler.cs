using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Persistence;
using NotificationService.Repositories.Interfaces;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, AppResult<NotificationDto>>
    {
        private readonly INotificationUnitOfWork _notificationUnitOfWork;
        private readonly IMapper _mapper;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        public CreateNotificationHandler(INotificationUnitOfWork notificationUnitOfWork, IMapper mapper, GrpcIdentityClient grpcIdentityClient)
        {
            _notificationUnitOfWork = notificationUnitOfWork;
            _mapper = mapper;
            _grpcIdentityClient = grpcIdentityClient;
        }
        public async Task<AppResult<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var noti = _mapper.Map<Entities.Notification>(request.CreateNotificationDto);
            if (string.IsNullOrEmpty(request.CreateNotificationDto.ReceiverId) && string.IsNullOrEmpty(request.CreateNotificationDto.GroupId))
            {
                return AppResult<NotificationDto>.Failure("ReceiverId or GroupId must be provided", 400);
            }
            var receivers = new List<string>();
            if (!string.IsNullOrEmpty(request.CreateNotificationDto.GroupId))
            {
                var group = await _notificationUnitOfWork.NotificationGroupRepository.GetByIdAsync(
                    request.CreateNotificationDto.GroupId,
                    q => q.Include(g => g.Members),
                    cancellationToken
                );
                if (group != null && group.Members != null)
                {
                    receivers.AddRange(group.Members.Select(m => m.UserId));
                }
            }
            else
            {
                receivers.Add(request.CreateNotificationDto.ReceiverId ?? throw new InvalidOperationException("ReceiverId must be provided when GroupId is not specified"));
            }
            foreach (var receiverId in receivers)
            {
                noti.AddRecipient(receiverId);
            }
            await _notificationUnitOfWork.NotificationRepository.AddAsync(noti);
            var res = await _notificationUnitOfWork.CommitAsync();
            
            if (!res) return AppResult<NotificationDto>.Failure("Failed to create notification", 500);

            var notificationDto = _mapper.Map<NotificationDto>(noti);
            var senderInfo = (await _grpcIdentityClient.GetUsersByIds(new List<string> { request.CreateNotificationDto.SenderId })).FirstOrDefault();
            notificationDto.SenderId = senderInfo?.UserId ?? request.CreateNotificationDto.SenderId;
            notificationDto.SenderName = senderInfo?.UserName;
            
            return AppResult<NotificationDto>.Success(notificationDto);
        }
    }
}