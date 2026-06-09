using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationService.Services;

namespace NotificationService.Workers
{
    public class UserInforSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public UserInforSyncWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var client = scope.ServiceProvider
                        .GetRequiredService<GrpcIdentityClient>();

                    await client.SyncUserInformation();

                    await Task.Delay(
                        TimeSpan.FromMinutes(5),
                        stoppingToken);
                    } // Sync every 5 minutes, can adjust as needed
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    await Task.Delay(
                        TimeSpan.FromSeconds(3),
                        stoppingToken);
                }
            }
        }
    }
}