using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Persistence;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Notification
{
    public class MarkAsReadListNotificationHandler : IRequestHandler<MarkAsReadListNotificationCommand, AppResult<Unit>>
    {
        private INotificationUnitOfWork _unitOfWork;
        public MarkAsReadListNotificationHandler(INotificationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(MarkAsReadListNotificationCommand request, CancellationToken cancellationToken)
        {
            var notifications = (await _unitOfWork.NotificationRepository.GetListAsync(
                predicate: n => n.Recipients.Any(r => r.UserId == request.UserId && request.NotificationIds.Contains(n.Id)),
                query: q => q.Include(n => n.Recipients),
                cancellationToken: cancellationToken
            )).ToList();

            if (notifications == null || !notifications.Any())
            {
                return AppResult<Unit>.Failure("No notifications found to mark as read.", 404);
            }

            if (request.NotificationIds.Count != notifications.Count)
            {
                return AppResult<Unit>.Failure("Some notifications do not belong to the user.", 403);
            }

            foreach (var notification in notifications)
            {
                var recipient = notification.Recipients.FirstOrDefault(r => r.UserId == request.UserId);
                if (recipient != null)
                {
                    recipient.IsRead = true;
                }
            }
            await _unitOfWork.CommitAsync(cancellationToken);
            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}