using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Data;
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}