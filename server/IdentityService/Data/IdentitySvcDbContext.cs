using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdentityService.Data
{
    public class IdentitySvcDbContext(DbContextOptions<IdentitySvcDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>()
            .HasData(
                new IdentityRole { Id = "9c47b469-293b-406c-8078-e82a8f2d7072", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "6b35a1c6-4a79-4154-bc92-7d65a5602676", Name = "Member", NormalizedName = "MEMBER" }
            );

            modelBuilder.Entity<RefreshToken>()
                .HasKey(r => r.Id);
        }
    }
}