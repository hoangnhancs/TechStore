using EmailService.Interfaces;
using EmailService.Services.Interface;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using NotificationService.Services.Sender;
using Shared.Core.EF.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Services.Order
{
    public class HandleOrderConfirmedHandler : IRequestHandler<HandleOrderConfirmedCommand, AppResult<Unit>>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateBuilder _templateBuilder;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly IMediator _mediator;
        private readonly INotificationServiceSender _notificationSender;
        private readonly INotificationUnitOfWork _unitOfWork;
        public HandleOrderConfirmedHandler(IEmailService emailService, 
            IEmailTemplateBuilder templateBuilder, 
            GrpcIdentityClient grpcIdentityClient, 
            IMediator mediator, 
            INotificationServiceSender notificationSender,
            INotificationUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _templateBuilder = templateBuilder;
            _grpcIdentityClient = grpcIdentityClient;
            _mediator = mediator;
            _notificationSender = notificationSender;
            _unitOfWork = unitOfWork;
        }
        public async Task<AppResult<Unit>> Handle(HandleOrderConfirmedCommand request, CancellationToken cancellationToken)
        {
            //send email to user
            string userOrderUrl = $"http://localhost:3000/my-orders/{request.Message.OrderId}";
            string adminOrderUrl = $"http://localhost:3000/admin/orders/{request.Message.OrderId}";
            var message = request.Message;
            var user = (await _unitOfWork.UserInformationRepository.GetListAsync(x => x.UserId == message.UserId)).FirstOrDefault();
            var body = await _templateBuilder.BuildAsync("OrderConfirmation", new
            {
                OrderNo = message.OrderNo,
                CustomerName = user?.DisplayName,

                OrderDate = message.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                Address = message.Address ?? "N/A",

                SubTotal = message.SubTotal.ToString("N0") + "₫",
                ShippingFee =message.ShippingCost.ToString("N0") + "₫",
                Discount = message.Discount.ToString("N0") + "₫",
                TotalPrice = message.Total.ToString("N0") + "₫",

                OrderUrl = userOrderUrl,

                Items = message.Items.Select(i => new
                {
                    Name = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.UnitPrice.ToString("N0") + "₫",
                    Total = (i.UnitPrice * i.Quantity).ToString("N0") + "₫",
                    ImageUrl = i.ProductImageUrl
                })
            });
            try
            {
                var recipientEmail = user?.UserEmail ?? throw new ArgumentNullException(nameof(user.UserEmail));
                await _emailService.SendEmailAsync(recipientEmail, "Xác nhận đơn hàng", body);
            }
            catch (Exception ex)
            {
                // Log lỗi gửi email nhưng không trả về lỗi để tránh ảnh hưởng đến trải nghiệm người dùng
                throw new Exception($"Failed to send order confirmation email: {ex.Message}");
            }
            //publish signalR notification to admin and user
            // Admin notification
            var systemUser = await _grpcIdentityClient.GetSystemUser();
            var adminGroup = (await _mediator.Send(new GetNotificationGroupByNameQuery { Name = NotificationGroups.AllAdminsNotiGroupName })).Value;
            if (adminGroup == null)
            {
                throw new ArgumentNullException(nameof(adminGroup), "Admin notification group not found");
            }

            var adminNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = $"Đơn hàng mới: {message.OrderNo}",
                    Message = $"Đơn hàng {message.OrderNo} vừa được đặt bởi {user?.DisplayName}.",
                    Category = "Order",
                    Type = "NewOrder",
                    ReferenceId = message.OrderId,
                    ReferenceType = "Order",
                    GroupId = adminGroup.Id, // Gửi đến nhóm admin
                    SenderId = systemUser?.UserId ?? throw new ArgumentNullException(nameof(systemUser)),
                    SenderName = systemUser?.UserName ?? "System",
                    SenderImageUrl = systemUser?.ImageUrl,
                }
            };
            var userNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = $"Đơn hàng của bạn đã được xác nhận: {message.OrderNo}",
                    Message = $"Cảm ơn bạn đã đặt hàng! Đơn hàng {message.OrderNo} của bạn đã được xác nhận và đang được xử lý.",
                    Category = "Order",
                    Type = "OrderPlaced",
                    ReferenceId = message.OrderId,
                    ReferenceType = "Order",
                    ReceiverId = message.UserId, // Gửi trực tiếp đến user
                    SenderId = systemUser?.UserId ?? throw new ArgumentNullException(nameof(systemUser)),
                    SenderName = systemUser?.UserName ?? "System",
                    SenderImageUrl = systemUser?.ImageUrl,
                }
            };

            //send notification email will be implemented in create notification handler, here we just create notification
      
            var adminNotiResult = await _mediator.Send(adminNotificationCommand, cancellationToken);
            

            //if (adminNotiResult.IsSuccess && adminNotiResult.Value != null)
            //{
            //    await _notificationSender.SendToGroupAsync(adminGroup.Name, adminNotiResult.Value);
            //}

            var userNotiResult = await _mediator.Send(userNotificationCommand, cancellationToken);
            //if (userNotiResult.IsSuccess && userNotiResult.Value != null)
            //{
            //    await _notificationSender.SendToUserAsync(message.UserId, userNotiResult.Value);
            //}

            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}