using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IdentityService.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Data
{
    public class DbInitializer
        {
        public static async Task SeedData(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Member", "Admin", "System" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            
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

                var system = new User
                {
                    DisplayName = "System",
                    UserName = "system",
                    Email = "system@techstore.com.vn",   
                    PhoneNumber = GenerateVietnamPhoneNumber(),
                    EmailConfirmed = true,  
                    IsAdmin = true
                };
                var result = await userManager.CreateAsync(system, "Pa$$w0rd");

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                await userManager.AddToRolesAsync(system, ["Member", "Admin", "System"]);

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