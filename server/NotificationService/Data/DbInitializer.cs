using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.RequestHelpers;
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
                    Name = NotificationGroups.AllAdminsNotiGroupName,
                    CreatedAt = DateTime.UtcNow,
                    Type = NotificationGroupType.Admins,
                };
                var userNotiGr = new NotificationGroup
                {
                    Name = NotificationGroups.AllUsersNotiGroupName,
                    CreatedAt = DateTime.UtcNow,
                    Type = NotificationGroupType.Users,
                };
                var allUsers = await grpcIdentityClient.GetAllUsers();
                adminNotiGr.Members = allUsers.Where(u => u.IsAdmin == true)
                    .Select(u => new NotificationGroupMember
                    {
                        NotificationGroupId = adminNotiGr.Id,
                        NotificationGroup = adminNotiGr,
                        UserId = u.UserId,
                        // UserName = u.UserName,
                        // UserImageUrl = u.UserEmail,
                        CreatedAt = DateTime.UtcNow,
                    }).ToList();
                userNotiGr.Members = allUsers.Where(u => u.IsAdmin == false)
                    .Select(u => new NotificationGroupMember
                    {
                        NotificationGroupId = userNotiGr.Id,
                        NotificationGroup = userNotiGr,
                        UserId = u.UserId,
                        // UserName = u.UserName,
                        // UserImageUrl = u.UserEmail,
                        CreatedAt = DateTime.UtcNow,
                    }).ToList();

                await context.NotificationGroups.AddRangeAsync(adminNotiGr, userNotiGr);
                await context.SaveChangesAsync();
            }
        }
    }
}