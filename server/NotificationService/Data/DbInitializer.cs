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
                
            }
        }
    }
}