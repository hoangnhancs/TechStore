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
    public class DeleteListNotificationsHandler : IRequestHandler<DeleteListNotificationsCommand, AppResult<Unit>>
    {
        private readonly INotificationUnitOfWork _unitOfWork;
        public DeleteListNotificationsHandler(INotificationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(DeleteListNotificationsCommand request, CancellationToken cancellationToken)
        {
            if (request.NotificationIds == null || !request.NotificationIds.Any())
            {
                return AppResult<Unit>.Failure("NotificationIds is required.", 400);
            }

            var notificationIds = request.NotificationIds.Distinct().ToList();

            var notifications = (await _unitOfWork.NotificationRepository.GetListAsync(
                predicate: n => notificationIds.Contains(n.Id) && n.Recipients.Any(r => r.UserId == request.UserId),
                query: q => q.Include(n => n.Recipients),
                cancellationToken: cancellationToken
            )).ToList();

            if (!notifications.Any())
            {
                return AppResult<Unit>.Failure("No notifications found to delete.", 404);
            }

            if (notificationIds.Count != notifications.Count)
            {
                return AppResult<Unit>.Failure("Some notifications do not belong to the user.", 403);
            }

            foreach (var notification in notifications)
            {
                var recipient = notification.Recipients.FirstOrDefault(r => r.UserId == request.UserId);
                if (recipient != null)                
                {
                    recipient.MarkAsDeleted();
                }
                else
                {
                    return AppResult<Unit>.Failure("Some notifications do not belong to the user. Operation was terminated.", 403);
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return AppResult<Unit>.Success(Unit.Value);

        }
    }
}