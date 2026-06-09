using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;

namespace NotificationService.Data
{
    public class NotificationSvcDbContext : DbContext
    {
        public NotificationSvcDbContext(DbContextOptions<NotificationSvcDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationGroup> NotificationGroups { get; set; }
        public DbSet<NotificationGroupMember> NotificationGroupMembers { get; set; }
        public DbSet<UserInformation> UserInformations { get; set; }
    }
}