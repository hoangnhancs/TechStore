using Contract.Order;
using EmailService.Interfaces;
using EmailService.Services.Interface;
using MassTransit;
using MediatR;
using NotificationService.DTOs;
using NotificationService.Services;
using NotificationService.Services.Notification;
using NotificationService.Services.NotificationGroup;
using NotificationService.Services.Order;

namespace NotificationService.Consumers
{
    public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateBuilder _templateBuilder;
        private readonly GrpcIdentityClient _grpcIdentityClient;
        private readonly IMediator _mediator;
        public OrderConfirmedConsumer(IEmailService emailService, 
            IEmailTemplateBuilder templateBuilder, 
            GrpcIdentityClient grpcIdentityClient, 
            IMediator mediator)
        {
            _emailService = emailService;
            _templateBuilder = templateBuilder;
            _grpcIdentityClient = grpcIdentityClient;
            _mediator = mediator;
        }
        public async Task Consume(ConsumeContext<OrderConfirmed> context)
        {
            await _mediator.Send(new HandleOrderConfirmedCommand { Message = context.Message });
        }
    }
}
