using Infrastructure.EF.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VoucherService.Entities;

namespace VoucherService.Data
{
    public class VoucherSvcDbContext : BaseDbContext
    {
        public VoucherSvcDbContext(DbContextOptions<VoucherSvcDbContext> options) : base(options)
        {
        }

        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<AppliedVoucher> AppliedVouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Voucher>(entity =>
            {
                entity.HasIndex(v => v.Code).IsUnique();

                entity.Property(v => v.ApplicableProductIds)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                    .HasColumnType("jsonb")
                    .Metadata.SetValueComparer(
                        new ValueComparer<List<string>?>(
                            (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                            c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                            c => c == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<string>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)
                        )
                    );
            });

            builder.Entity<AppliedVoucher>(entity =>
            {
                entity.HasIndex(a => a.OrderId).IsUnique();
            });

            // MassTransit Outbox tables
            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            builder.AddOutboxStateEntity();
        }
    }
}
