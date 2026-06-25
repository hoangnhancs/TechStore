using EmailService.Interfaces;
using EmailService.Services.Interface;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using Shared.Core.EF.Application;
using static NotificationService.Entities.Notification;

namespace NotificationService.Services.Order
{
    public class HandleOrderCancelledHandler : IRequestHandler<HandleOrderCancelledCommand, AppResult<Unit>>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateBuilder _templateBuilder;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly IMediator _mediator;
        private readonly INotificationUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public HandleOrderCancelledHandler(
            IEmailService emailService,
            IEmailTemplateBuilder templateBuilder,
            GrpcIdentityClient grpcIdentityClient,
            IMediator mediator,
            INotificationUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _templateBuilder = templateBuilder;
            _grpcIdentityClient = grpcIdentityClient;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _configuration = configuration;

        }

        public async Task<AppResult<Unit>> Handle(HandleOrderCancelledCommand request, CancellationToken cancellationToken)
        {
            if (request.Message == null)
                return AppResult<Unit>.Failure("Message is null", 400);

            var message = request.Message;
            var user = (await _unitOfWork.UserInformationRepository.GetListAsync(x => x.UserId == message.UserId)).FirstOrDefault();

            var paymentMethodDisplay = message.PaymentMethod.ToLower() switch
            {
                "cashondelivery" => "Thanh toán khi nhận hàng (COD)",
                "creditcard"     => "Thẻ tín dụng",
                "momo"           => "MoMo",
                "vnpay"          => "VNPay",
                "banktransfer"   => "Chuyển khoản ngân hàng",
                _                => message.PaymentMethod
            };

            var body = await _templateBuilder.BuildAsync("OrderCancelled", new
            {
                message.OrderNo,
                CustomerName = user?.DisplayName,
                message.Reason,
                CancelledAt = message.CancelledAt.ToString("dd/MM/yyyy HH:mm:ss"),
                PaymentMethod = paymentMethodDisplay,
                ShopUrl = _configuration.GetValue<string>("ClientUrl") ?? throw new ArgumentNullException("ClientUrl configuration is missing")
            });

            try
            {
                var recipientEmail = user?.UserEmail ?? throw new ArgumentNullException(nameof(user));
                await _emailService.SendEmailAsync(recipientEmail, $"Đơn hàng #{message.OrderNo} đã bị hủy", body);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send order cancelled email: {ex.Message}");
            }

            var systemUser = await _grpcIdentityClient.GetSystemUser();
            var adminGroup = (await _mediator.Send(new GetNotificationGroupByNameQuery { Name = NotificationGroups.AllAdminsNotiGroupName })).Value;
            if (adminGroup == null)
                throw new ArgumentNullException(nameof(adminGroup), "Admin notification group not found");

            var adminNotificationCommand = new CreateNotificationCommand
            {
                CreateNotificationDto = new CreateNotificationDto
                {
                    Title = $"Đơn hàng bị hủy: #{message.OrderNo}",
                    Message = $"Đơn hàng #{message.OrderNo} đã bị hủy. Lý do: {message.Reason}",
                    Category = NotificationCategory.Order.ToString(),
                    Type = NotificationType.OrderCancelled.ToString(),
                    ReferenceId = message.OrderId,
                    ReferenceType = NotificationReferenceType.Order.ToString(),
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
                    Title = $"Đơn hàng #{message.OrderNo} đã bị hủy",
                    Message = $"Đơn hàng #{message.OrderNo} của bạn đã bị hủy. Lý do: {message.Reason}",
                    Category = "Order",
                    Type = "OrderCancelled",
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
