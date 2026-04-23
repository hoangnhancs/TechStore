using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.DTOs;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using NotificationService.Services.NotificationGroupMember;

namespace NotificationService.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly IMediator _mediator;

        public NotificationHub(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task JoinNotificationGroup()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ReviewError", "User not authenticated");
                Console.WriteLine("User not authenticated");
                return;
            }

            //admin thi only join group admin
            var notiGrsResult = await _mediator.Send(new GetNotificationGroupsByUserIdQuery { UserId = userId });
            if (notiGrsResult.IsSuccess && notiGrsResult.Value != null)
            {
                foreach (var group in notiGrsResult.Value)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, group.Name);
                    Console.WriteLine($"Client {userId} joined group {group.Name}");
                }
            }

            // var roles = Context.User?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            //ca nhan chi dung cho ca nhan

            await Groups.AddToGroupAsync(Context.ConnectionId, $"{userId}-notifications");
            Console.WriteLine($"Client {userId} joined group {userId}-notifications");
            
        }

        public async Task LeaveNotificationGroup()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }
            var notiGrsResult = await _mediator.Send(new GetNotificationGroupsByUserIdQuery { UserId = userId });
            if (notiGrsResult.IsSuccess && notiGrsResult.Value != null)
            {
                foreach (var group in notiGrsResult.Value)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.Name);
                    Console.WriteLine($"Client {userId} left group {group.Name}");
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{userId}-notifications");
        }


        public async Task SendNotification(CreateNotificationDto dto)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("NotificationError", "User not authenticated");
                return;
            }

            if (string.IsNullOrEmpty(dto.ReceiverId) && string.IsNullOrEmpty(dto.GroupId))
            {
                await Clients.Caller.SendAsync("NotificationError", "ReceiverId or GroupId must be provided");
                return;
            }
            // var roles = Context.User?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            //tao notification truoc de lay value gui qua signalR
            var noti = await _mediator.Send(new CreateNotificationCommand { CreateNotificationDto = dto });
            if (!noti.IsSuccess)
            {
                await Clients.Caller.SendAsync("NotificationError", "Failed to create notification");
                return;
            }

            if (!String.IsNullOrEmpty(dto.GroupId))
            {
                var reqRes = await _mediator.Send(new GetNotificationGroupByIdQuery { GroupId = dto.GroupId });
                if (reqRes.IsSuccess && reqRes.Value != null)
                {
                    var group = reqRes.Value;
                    await Clients.Group($"{group.Name}-notifications").SendAsync("ReceiveNotification", noti.Value);
                    Console.WriteLine($"Sent notification to group {group.Name}-notifications");
                }
            }
            else
            {
                await Clients.Group($"{dto.ReceiverId}-notifications").SendAsync("ReceiveNotification", noti.Value);
                Console.WriteLine($"Sent notification to group {dto.ReceiverId}-notifications");
            }

            var command = new CreateNotificationCommand
            {
                CreateNotificationDto = dto
            };
            var result = await _mediator.Send(command);
            if (String.IsNullOrEmpty(dto.ReceiverId))
            {
                await Clients.Group("admin-notifications").SendAsync("ReceiveNotification", result.Value);
                Console.WriteLine($"Sent notification to group admin-notifications");
            }
            else if (String.IsNullOrEmpty(dto.GroupId))
            {
                await Clients.Group($"{dto.ReceiverId}-notifications").SendAsync("ReceiveNotification", result.Value);
                Console.WriteLine($"Sent notification to group {dto.ReceiverId}-notifications");
            }

        }
        public async Task LoadAllNotifications()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("NotificationError", "User not authenticated");
                return;
            }

            var notifications = await _mediator.Send(new GetNotificationByUserIdQuery { UserId = userId });
            if (!notifications.IsSuccess)
            {
                await Clients.Caller.SendAsync("NotificationError", string.IsNullOrEmpty(notifications.Error) ? "Failed to load notifications" : notifications.Error);
                return;
            }
            await Clients.Caller.SendAsync("ReceiveAllNotifications", notifications.Value);
        }

    public async Task MarkAsReadListNotifications(List<string> notificationIds)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("NotificationError", "User not authenticated");
            return;
        }

        var res = await _mediator.Send(new MarkAsReadListNotificationCommand { UserId = userId, NotificationIds = notificationIds });

        if (!res.IsSuccess)
        {
            await Clients.Caller.SendAsync("NotificationError", string.IsNullOrEmpty(res.Error) ? "Have not found some notifications." : res.Error);
            return;
        }

        await Clients.Caller.SendAsync("ReceiveNotificationsRead", notificationIds);
    }

        public async Task DeleteListNotifications(List<string> notificationIds)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("NotificationError", "User not authenticated");
                return;
            }

            var res = await _mediator.Send(new DeleteListNotificationsCommand { UserId = userId, NotificationIds = notificationIds });
            if (!res.IsSuccess)
            {
                await Clients.Caller.SendAsync("NotificationError", string.IsNullOrEmpty(res.Error) ? "Have not found some notifications." : res.Error);
                return;
            }

            await Clients.Caller.SendAsync("ReceiveNotificationsDeleted", notificationIds);
        }
    }
}