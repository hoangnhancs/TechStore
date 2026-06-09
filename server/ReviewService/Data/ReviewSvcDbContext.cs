using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.EF.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReviewService.Entities;

namespace ReviewService.Data
{
    public class ReviewSvcDbContext : BaseDbContext
    {
        public ReviewSvcDbContext(DbContextOptions<ReviewSvcDbContext> options) : base(options)
        {
        }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserInformation> UserInformations { get; set; }
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