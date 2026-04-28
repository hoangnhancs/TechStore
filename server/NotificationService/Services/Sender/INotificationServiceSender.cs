using NotificationService.DTOs;

namespace NotificationService.Services.Sender
{
    public interface INotificationServiceSender
    {
        /// <summary>
        /// Gửi notification qua SignalR đến personal group của một user cụ thể: "{userId}-notifications"
        /// </summary>
        Task SendToUserAsync(string userId, NotificationDto notification);

        /// <summary>
        /// Gửi notification qua SignalR đến một group (ví dụ: "admin-notifications")
        /// </summary>
        Task SendToGroupAsync(string groupName, NotificationDto notification);
    }
}
