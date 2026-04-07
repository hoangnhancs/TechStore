using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace PaymentService.Entities
{
    public class Payment : BaseEntity<string>
    {
        public required string UserId { get; set; } 
        public required string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        [Column(TypeName = "varchar(20)")]
        public required PaymentMethodType PaymentMethod { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        [Column(TypeName = "varchar(20)")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Processing;
        public Payment() : base(Guid.NewGuid().ToString())
        {
        }
        public static Payment CreatePayment(string userId, string orderId, decimal amount, PaymentMethodType paymentMethod)
        {
            return new Payment
            {
                UserId = userId,
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = PaymentStatus.Processing
            };
        }
        public enum PaymentStatus
        {
            Processing,
            Succeeded,
            Failed,
            Canceled,
        }
        public enum PaymentMethodType
        {
            CashOnDelivery,
            CreditCard,
            Momo,
            VNPay,
            BankTransfer
        }
    }
}