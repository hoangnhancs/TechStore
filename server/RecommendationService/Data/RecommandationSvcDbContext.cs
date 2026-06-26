using Infrastructure.EF.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Entities;

namespace RecommendationService.Data
{
    public class RecommandationSvcDbContext : BaseDbContext
    {
        public RecommandationSvcDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ProductVectorEmbedding> ProductVectorEmbeddings { get; set; }
        public DbSet<UserActionTracking> UserActionTrackings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
