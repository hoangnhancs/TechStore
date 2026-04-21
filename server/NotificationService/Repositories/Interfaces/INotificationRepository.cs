using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace NotificationService.Repositories.Interfaces
{
    public interface INotificationRepository : IBaseEFRepository<Notification, string>
    {
    }
}