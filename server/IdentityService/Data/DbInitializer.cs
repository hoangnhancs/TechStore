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
                        PhoneNumber = GenerateVietnamPhoneNumber(),
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
                    PhoneNumber = GenerateVietnamPhoneNumber(),
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
                    PhoneNumber = GenerateVietnamPhoneNumber(),
                    EmailConfirmed = true,
                    IsAdmin = true  
                };

                await userManager.CreateAsync(admin2, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin2, ["Member", "Admin"]);
            }
        }
        public static string GenerateVietnamPhoneNumber()
        {
            var random = new Random();

            // Các đầu số phổ biến VN
            string[] prefixes = { "032", "033", "034", "035", "036", "037", "038", "039",
                                "070", "076", "077", "078", "079",
                                "081", "082", "083", "084", "085",
                                "056", "058",
                                "091", "094", "088" };

            string prefix = prefixes[random.Next(prefixes.Length)];

            string remaining = random.Next(0, 9999999).ToString("D7");

            return prefix + remaining;
        }
    }
}