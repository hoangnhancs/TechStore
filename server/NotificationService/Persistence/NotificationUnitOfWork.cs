using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.UnitOfWork;
using NotificationService.Data;
using NotificationService.Repositories;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Persistence
{
    public class NotificationUnitOfWork : UnitOfWork<NotificationSvcDbContext>, INotificationUnitOfWork
    {

        private INotificationRepository? _notificationRepository;
        public INotificationRepository NotificationRepository =>
            _notificationRepository ??= new NotificationRepository(_dbContext);

        public NotificationUnitOfWork(NotificationSvcDbContext dbContext) : base(dbContext)
        {

        }
    }
}