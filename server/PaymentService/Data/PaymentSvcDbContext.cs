using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Entities;

namespace PaymentService.Data
{
    public class PaymentSvcDbContext : BaseDbContext
    {
        public PaymentSvcDbContext(DbContextOptions<PaymentSvcDbContext> options) : base(options)
        {
        }
        public DbSet<Payment> Payments { get; set; } 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // MassTransit Outbox tables
            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            builder.AddOutboxStateEntity();
        }
    }
}