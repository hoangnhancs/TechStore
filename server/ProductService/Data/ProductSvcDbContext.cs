using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Entities;

namespace ProductService.Data
{
    public class ProductSvcDbContext : DbContext
    {
        public ProductSvcDbContext(DbContextOptions<ProductSvcDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<FilterTag> FilterTags { get; set; }
        public DbSet<FilterTagValue> FilterTagValues { get; set; }
        public DbSet<ProductFilterTagValue> ProductFilterTagValues { get; set; }
        public DbSet<ProductDisplayTag> ProductDisplayTags { get; set; }
        public DbSet<ProductVectorEmbedding> ProductVectorEmbeddings { get; set; }
        public DbSet<BannerImage> BannerImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasPrecision(3, 2);
            modelBuilder.Entity<ProductFilterTagValue>()
                .HasOne(pftv => pftv.FilterTagValue)
                .WithMany()
                .HasForeignKey(pftv => pftv.FilterTagValueId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}