using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService
{
    public class DbInitializer
        {
        public static async Task SeedData(IdentitySvcDbContext context, UserManager<User> userManager)
        {
            if (!userManager.Users.Any())
            {
                var userNames = new List<string>() { "Bob", "Jane", "Tom", "Erik", "Philip", "Ralph", "Join", "Sam", "Ken", "Timmy" };

                foreach (var userName in userNames)
                {
                    var user = new User
                    {
                        DisplayName = userName,
                        UserName = $"{userName.ToLower()}@gmail.com",
                        Email = $"{userName.ToLower()}@gmail.com",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                    await userManager.AddToRoleAsync(user, "Member");
                }

                var admin1 = new User
                {
                    DisplayName = "Admin",
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    IsAdmin = true
                };

                await userManager.CreateAsync(admin1, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin1, ["Member", "Admin"]);

                var admin2 = new User
                {
                    DisplayName = "Hoàng Nhân",
                    UserName = "thaihoangnhantk17lqd@gmail.com",
                    Email = "thaihoangnhantk17lqd@gmail.com",
                    IsAdmin = true
                };

                await userManager.CreateAsync(admin2, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin2, ["Member", "Admin"]);
            }
        }
    }
}