using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.Services;

namespace NotificationService.Data
{
    public class DbInitializer
    {
        public static async Task SeedData(
            NotificationSvcDbContext context,
            ILogger<DbInitializer> logger,
            GrpcIdentityClient grpcIdentityClient)
        {
            var notificationGroup = await context.NotificationGroups
                .FirstOrDefaultAsync(x => x.Name == "admin-notifications");

            if (notificationGroup == null)
            {
                notificationGroup = new NotificationGroup
                {
                    Name = "admin-notifications",
                };

                await context.NotificationGroups.AddAsync(notificationGroup);
                logger.LogInformation("Add NotificationGroup!");
            }
            else
            {
                logger.LogInformation("Notification group already exists");
            }

            var admins = await grpcIdentityClient.GetConfiguredAdminUsers();
            if (admins.Count == 0)
            {
                logger.LogInformation("No admin users configured for NotificationGroup seed.");
                await context.SaveChangesAsync();
                return;
            }

            var existingUserIds = await context.NotificationGroupMembers
                .Where(x => x.NotificationGroupId == notificationGroup.Id)
                .Select(x => x.UserId)
                .ToListAsync();

            var membersToAdd = admins
                .Where(user => !existingUserIds.Contains(user.UserId))
                .Select(user => new NotificationGroupMember
                {
                    NotificationGroupId = notificationGroup.Id,
                    UserId = user.UserId,
                })
                .ToList();

            if (membersToAdd.Count > 0)
            {
                await context.NotificationGroupMembers.AddRangeAsync(membersToAdd);
                logger.LogInformation("Add admins to NotificationGroup!");
            }
            else
            {
                logger.LogInformation("Notification group member already exists");
            }

            await context.SaveChangesAsync();
        }
    }
}