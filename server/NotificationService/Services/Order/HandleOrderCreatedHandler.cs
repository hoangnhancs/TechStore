using EmailService.Interfaces;
using EmailService.Services.Interface;
using MediatR;
using Microsoft.Extensions.Configuration;
using NotificationService.DTOs;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using Shared.Core.EF.Application;

namespace NotificationService.Services.Order
{
    public class HandleOrderCreatedHandler : IRequestHandler<HandleOrderCreatedCommand, AppResult<Unit>>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateBuilder _templateBuilder;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly IMediator _mediator;
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public HandleOrderCreatedHandler(IEmailService emailService,
            IEmailTemplateBuilder templateBuilder,
            GrpcIdentityClient grpcIdentityClient,
            IMediator mediator,
            INotificationUnitOfWork unitOfWork,
            IConfiguration configuration    )
        {
            _emailService = emailService;
            _templateBuilder = templateBuilder;
            _grpcIdentityClient = grpcIdentityClient;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _configuration = configuration;

        }

        public async Task<AppResult<Unit>> Handle(HandleOrderCreatedCommand request, CancellationToken cancellationToken)
        {
            if (request.Message == null)
                return AppResult<Unit>.Failure("Message is null", 400);

            var message = request.Message;

            // Chỉ xử lý đơn COD — online payment sẽ nhận email sau khi thanh toán thành công
            if (!message.PaymentMethod.Equals("cashondelivery", StringComparison.OrdinalIgnoreCase))
                return AppResult<Unit>.Success(Unit.Value);

            var user = (await _unitOfWork.UserInformationRepository.GetListAsync(x => x.UserId == message.UserId)).FirstOrDefault();
            string userOrderUrl = $"{_configuration.GetValue<string>("ClientUrl") ?? throw new ArgumentNullException("ClientUrl configuration is missing")}/my-orders/{message.OrderId}";

            var body = await _templateBuilder.BuildAsync("OrderCreated", new
            {
                OrderId = message.OrderId,
                CustomerName = user?.DisplayName,
                OrderDate = message.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                Address = message.ShippingAddress ?? "N/A",
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
                var recipientEmail = user?.UserEmail ?? throw new ArgumentNullException(nameof(user));
                await _emailService.SendEmailAsync(recipientEmail, "Đặt hàng thành công - Chờ xác nhận", body);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send order created email: {ex.Message}");
            }

            var systemUser = await _grpcIdentityClient.GetSystemUser();
            var adminGroup = (await _mediator.Send(new GetNotificationGroupByNameQuery { Name = NotificationGroups.AllAdminsNotiGroupName })).Value;
            if (adminGroup == null)
                throw new ArgumentNullException(nameof(adminGroup), "Admin notification group not found");

            var adminNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = $"Đơn hàng mới: #{message.OrderId}",
                    Message = $"Đơn hàng #{message.OrderId} vừa được đặt bởi {user?.DisplayName}.",
                    Category = "Order",
                    Type = "NewOrder",
                    ReferenceId = message.OrderId,
                    ReferenceType = "Order",
                    GroupId = adminGroup.Id,
                    SenderId = systemUser?.UserId ?? throw new ArgumentNullException(nameof(systemUser)),
                    SenderName = systemUser?.UserName ?? "System",
                    SenderImageUrl = systemUser?.ImageUrl,
                }
            };

            var userNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = $"Đặt hàng thành công: #{message.OrderId}",
                    Message = $"Đơn hàng #{message.OrderId} của bạn đã được tiếp nhận và đang chờ xác nhận.",
                    Category = "Order",
                    Type = "OrderPlaced",
                    ReferenceId = message.OrderId,
                    ReferenceType = "Order",
                    ReceiverId = message.UserId,
                    SenderId = systemUser?.UserId ?? throw new ArgumentNullException(nameof(systemUser)),
                    SenderName = systemUser?.UserName ?? "System",
                    SenderImageUrl = systemUser?.ImageUrl,
                }
            };

            await _mediator.Send(adminNotificationCommand, cancellationToken);
            await _mediator.Send(userNotificationCommand, cancellationToken);

            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}
