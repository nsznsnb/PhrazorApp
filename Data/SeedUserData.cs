﻿using Microsoft.AspNetCore.Identity;
using PhrazorApp.Data;
using PhrazorApp.Options;

namespace PhrazorApp.Data
{
    public static class SeedUserData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, SeedUserOptions options)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ロール作成（指定されたロールのみ）
            if (!await roleManager.RoleExistsAsync(options.Role))
            {
                await roleManager.CreateAsync(new IdentityRole(options.Role));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));

            }

            // ユーザー作成
            var user = await userManager.FindByEmailAsync(options.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = options.Name,
                    Email = options.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, options.Password);
                if (!result.Succeeded)
                {
                    throw new Exception("初期ユーザー作成に失敗: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(user, options.Role);
            }
        }
    }



}
