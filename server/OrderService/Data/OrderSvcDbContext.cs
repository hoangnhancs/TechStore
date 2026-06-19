using Contract.Order;
using Infrastructure.EF.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Entities;
using OrderService.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using static OrderService.Entities.Order;

namespace OrderService.Data
{
    public class OrderSvcDbContext : BaseDbContext
    {
        public OrderSvcDbContext(DbContextOptions<OrderSvcDbContext> options) : base(options)
        {
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        
        // Saga State Machine persistence
        public DbSet<OrderSagaState> OrderSagaStates { get; set; }
        public DbSet<UserInformation> UserInformations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(OrderSvcDbContext).Assembly);
            // builder.Entity<Order>()
            //     .HasOne(o => o.Shipment)
            //     .WithOne(s => s.Order)
            //     .HasForeignKey<Shipment>(s => s.OrderId); // 👈 FIX ở đây
            // MassTransit Saga State Map
            builder.Entity<OrderSagaState>(entity =>
            {
                entity.HasKey(s => s.CorrelationId);

                // Store Items as JSON column with ValueComparer
                entity.Property(s => s.Items)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<OrderItemEvent>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<OrderItemEvent>()
                    )
                    .HasColumnType("jsonb")
                    .Metadata.SetValueComparer(
                        new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<OrderItemEvent>>(
                            (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                            c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                            c => System.Text.Json.JsonSerializer.Deserialize<List<OrderItemEvent>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null) ?? new List<OrderItemEvent>()
                        )
                    );
            });
            builder.Entity<UserInformation>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            builder.Entity<Order>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.UserId);
            // MassTransit Outbox tables
            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            builder.AddOutboxStateEntity();
        }
    }
}