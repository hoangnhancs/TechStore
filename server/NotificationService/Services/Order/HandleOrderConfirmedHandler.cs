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
using static NotificationService.Entities.Notification;

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
        private readonly IConfiguration _configuration;
        public HandleOrderConfirmedHandler(IEmailService emailService, 
            IEmailTemplateBuilder templateBuilder, 
            GrpcIdentityClient grpcIdentityClient, 
            IMediator mediator, 
            INotificationServiceSender notificationSender,
            INotificationUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _templateBuilder = templateBuilder;
            _grpcIdentityClient = grpcIdentityClient;
            _mediator = mediator;
            _notificationSender = notificationSender;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        public async Task<AppResult<Unit>> Handle(HandleOrderConfirmedCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            string userOrderUrl = $"{_configuration.GetValue<string>("ClientUrl") ?? throw new ArgumentNullException("ClientUrl configuration is missing")}/my-orders/{message.OrderId}";

            var user = (await _unitOfWork.UserInformationRepository.GetListAsync(x => x.UserId == message.UserId)).FirstOrDefault();

            bool isCod = message.PaymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase);
            string paymentMethodDisplay = isCod ? "Thanh toán khi nhận hàng (COD)" : "Thẻ tín dụng / Thanh toán online";

            var body = await _templateBuilder.BuildAsync("OrderConfirmation", new
            {
                OrderNo = message.OrderNo,
                CustomerName = user?.DisplayName,
                OrderDate = message.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                Address = message.Address ?? "N/A",
                PaymentMethod = paymentMethodDisplay,
                IsCod = isCod,
                SubTotal = message.SubTotal.ToString("N0") + "₫",
                ShippingFee = message.ShippingCost.ToString("N0") + "₫",
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
                var recipientEmail = user?.UserEmail ?? throw new InvalidOperationException("User email not found");
                string emailSubject = isCod
                    ? $"Đơn hàng #{message.OrderNo} đã được xác nhận"
                    : $"Thanh toán thành công - Đơn hàng #{message.OrderNo}";
                await _emailService.SendEmailAsync(recipientEmail, emailSubject, body);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send order confirmation email: {ex.Message}");
            }

            var systemUser = await _grpcIdentityClient.GetSystemUser();
            var adminGroup = (await _mediator.Send(new GetNotificationGroupByNameQuery { Name = NotificationGroups.AllAdminsNotiGroupName })).Value;
            if (adminGroup == null)
                throw new ArgumentNullException(nameof(adminGroup), "Admin notification group not found");

            string adminMessage = isCod
                ? $"Đơn hàng COD #{message.OrderNo} của {user?.DisplayName} đã được xác nhận và đang xử lý."
                : $"Đơn hàng online #{message.OrderNo} của {user?.DisplayName} đã thanh toán thành công.";

            string userMessage = isCod
                ? $"Đơn hàng #{message.OrderNo} của bạn đã được xác nhận và đang được chuẩn bị."
                : $"Thanh toán thành công! Đơn hàng #{message.OrderNo} của bạn đang được xử lý.";

            var adminNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = isCod ? $"Đã xác nhận đơn hàng: #{message.OrderNo}" : $"Đơn hàng đã được đặt: #{message.OrderNo}",
                    Message = adminMessage,
                    Category = NotificationCategory.Order.ToString(),
                    Type = NotificationType.OrderConfirmed.ToString(),
                    ReferenceId = message.OrderId,
                    ReferenceType = NotificationReferenceType.Order.ToString(),
                    GroupId = adminGroup.Id,
                    SenderId = systemUser?.UserId ?? throw new InvalidOperationException("System user not found"),
                    SenderName = systemUser?.UserName ?? "System",
                    SenderImageUrl = systemUser?.ImageUrl,
                }
            };
            await _mediator.Send(adminNotificationCommand, cancellationToken);


            var userNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = isCod ? $"Đơn hàng đã được xác nhận: #{message.OrderNo}" : $"Đơn hàng đã được đặt: #{message.OrderNo}",
                    Message = userMessage,
                    Category = "Order",
                    Type = "OrderPlaced",
                    ReferenceId = message.OrderId,
                    ReferenceType = "Order",
                    ReceiverId = message.UserId,
                    SenderId = systemUser?.UserId ?? throw new InvalidOperationException("System user not found"),
                    SenderName = systemUser?.UserName ?? "System",
                    SenderImageUrl = systemUser?.ImageUrl,
                }
            };
            await _mediator.Send(userNotificationCommand, cancellationToken);

            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}