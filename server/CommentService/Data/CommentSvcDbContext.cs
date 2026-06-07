using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.Entities;
using Infrastructure.EF.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Data
{
    public class CommentSvcDbContext : BaseDbContext
    {
        public CommentSvcDbContext(DbContextOptions<CommentSvcDbContext> options) : base(options)
        {
        }
        public DbSet<Comment> Comments { get; set; }
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