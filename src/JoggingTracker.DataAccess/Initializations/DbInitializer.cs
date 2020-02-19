using System;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JoggingTracker.DataAccess.Initializations
{
    public static class DbInitializer
    {
        private const string AdminUserName = "admin";
        private const string DefaultPassword = "P@ssw0rd1!";
        
        public static void Initialize(IServiceProvider services)
        {
            
            var context = services.GetRequiredService<JoggingTrackerDataContext>();
            context.Database.Migrate(); // apply all migrations
            
            // seed data 
            var userManager = services.GetRequiredService<UserManager<UserDb>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            SeedData(userManager, roleManager);
        }
        
        private static void SeedData(UserManager<UserDb> userManager, RoleManager<IdentityRole> roleManager)
        { 
            SeedRoles(roleManager); 
            SeedUsers(userManager);
        }
 
        private static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in UserRoles.AllRoles)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    IdentityRole role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName
                    };
                    roleManager.CreateAsync(role).Wait();
                }
            }
        }
        
        private static void SeedUsers(UserManager<UserDb> userManager)
        {
            var defaultAdminUserName = AdminUserName;
            var adminUser = userManager.FindByNameAsync(defaultAdminUserName).Result;
            
            if (adminUser == null)
            {
                UserDb user = new UserDb
                {
                    UserName = defaultAdminUserName
                };
                
                IdentityResult result = userManager.CreateAsync(user, DefaultPassword).Result;
 
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, UserRoles.Admin).Wait();
                }
            }
        }
    }
}