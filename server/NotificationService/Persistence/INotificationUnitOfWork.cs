using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationService.Repositories.Interfaces;
using Shared.Core.EF.UnitOfWork;

namespace NotificationService.Persistence
{
    public interface INotificationUnitOfWork : IUnitOfWork
    {
        public INotificationRepository NotificationRepository { get; }
    }
}