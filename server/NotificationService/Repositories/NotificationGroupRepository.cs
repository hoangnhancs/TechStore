using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Repositories;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories
{
    public class NotificationGroupRepository : BaseEFRepository<NotificationGroup, string, NotificationSvcDbContext>, INotificationGroupRepository
    {
        public NotificationGroupRepository(NotificationSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}