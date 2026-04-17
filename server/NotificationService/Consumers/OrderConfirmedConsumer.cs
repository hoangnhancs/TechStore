using Contract;
using EmailService.Interfaces;
using EmailService.Services.Interface;
using MassTransit;

namespace NotificationService.Consumers
{
    public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateBuilder _templateBuilder;
        public OrderConfirmedConsumer(IEmailService emailService, IEmailTemplateBuilder templateBuilder)
        {
            _emailService = emailService;
            _templateBuilder = templateBuilder;
        }
        public async Task Consume(ConsumeContext<OrderConfirmed> context)
        {
            var message = context.Message;
            var body = await _templateBuilder.BuildAsync("OrderConfirmation", new
            {
                OrderNo = message.OrderNo,
                CustomerName = message.UserName,

                OrderDate = message.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                Address = message.Address ?? "N/A",

                SubTotal = message.SubTotal.ToString("N0") + "₫",
                ShippingFee =message.ShippingCost.ToString("N0") + "₫",
                Discount = message.Discount.ToString("N0") + "₫",
                TotalPrice = message.Total.ToString("N0") + "₫",

                OrderUrl = $"http://localhost:3000/my-orders/{message.OrderId}",

                Items = message.Items.Select(i => new
                {
                    Name = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.UnitPrice.ToString("N0") + "₫",
                    Total = (i.UnitPrice * i.Quantity).ToString("N0") + "₫",
                    ImageUrl = i.ProductImageUrl
                })
            });

            await _emailService.SendEmailAsync("thaihoangnhantk17lqd@gmail.com", "Xác nhận đơn hàng", body);
        }
    }
}
