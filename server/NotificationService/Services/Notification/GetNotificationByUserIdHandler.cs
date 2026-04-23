using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class GetNotificationByUserIdHandler : IRequestHandler<GetNotificationByUserIdQuery, AppResult<List<NotificationDto>>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetNotificationByUserIdHandler(INotificationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AppResult<List<NotificationDto>>> Handle(GetNotificationByUserIdQuery request, CancellationToken cancellationToken)
        {
            var notifications = (await _unitOfWork.NotificationRepository.GetListAsync(
                predicate: n => n.Recipients.Any(r => r.UserId == request.UserId),
                cancellationToken: cancellationToken
            )).OrderByDescending(n => n.CreatedAt).ToList();

            if (notifications == null || !notifications.Any())
            {
                return AppResult<List<NotificationDto>>.Success(new List<NotificationDto>()); // Return empty list if no notifications found
            }

            var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);
            return AppResult<List<NotificationDto>>.Success(notificationDtos);
        }
    }
}