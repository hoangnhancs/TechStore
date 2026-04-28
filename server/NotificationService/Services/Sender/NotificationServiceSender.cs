using Microsoft.AspNetCore.SignalR;
using NotificationService.DTOs;
using NotificationService.SignalR;

namespace NotificationService.Services.Sender
{
    public class NotificationServiceSender : INotificationServiceSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationServiceSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(string userId, NotificationDto notification)
        {
            await _hubContext.Clients
                .Group($"{userId}-notifications")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendToGroupAsync(string groupName, NotificationDto notification)
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
