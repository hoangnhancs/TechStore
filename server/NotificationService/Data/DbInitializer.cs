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
            if (!context.NotificationGroups.Any())
            {
                var adminNotiGr = new NotificationGroup
                {
                    Name = "admin-notifications",
                };
                var userNotiGr = new NotificationGroup
                {
                    Name = "user-notifications",
                };
                var allUsers = await grpcIdentityClient.GetAllUsers();
                adminNotiGr.Members = allUsers.Where(u => u.IsAdmin == true)
                    .Select(u => new NotificationGroupMember
                    {
                        NotificationGroupId = adminNotiGr.Id,
                        NotificationGroup = adminNotiGr,
                        UserId = u.UserId,
                    }).ToList();
                userNotiGr.Members = allUsers.Where(u => u.IsAdmin == false)
                    .Select(u => new NotificationGroupMember
                    {
                        NotificationGroupId = userNotiGr.Id,
                        NotificationGroup = userNotiGr,
                        UserId = u.UserId,
                    }).ToList();

                await context.NotificationGroups.AddRangeAsync(adminNotiGr, userNotiGr);
                await context.SaveChangesAsync();
            }
        }
    }
}