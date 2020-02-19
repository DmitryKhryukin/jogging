using System;
using System.Collections.Generic;
using FluentAssertions;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JoggingTracker.Api.Tests
{
    public class FakeDbUtilities
    {
        public const string UserPassword = "password";

        public static UserDb adminUser = new UserDb()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testUserName1Admin",
        };
        
        public static UserDb managerUser = new UserDb()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testUserName2Usermanager",
        };

        public static UserDb regularUser = new UserDb()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testUserName3Regular",
        };
        
        public static readonly List<UserDb> SeedUsers = new List<UserDb>()
        {
            adminUser,
            managerUser,
            regularUser
        };

        public static readonly List<RunDb> SeedRegularUserRuns = new List<RunDb>()
        {
            new RunDb()
            {
                Id = 1,
                UserId = regularUser.Id,
                Distance = 1,
                Time = 2,
                Date = new DateTime(2020, 2, 12),
                Latitude = 2,
                Longitude = 2
            },
            new RunDb()
            {
                Id = 2,
                UserId = regularUser.Id,
                Distance = 1,
                Time = 2,
                Date = new DateTime(2020, 2, 5),
                Latitude = 2,
                Longitude = 2
            }
            ,
            new RunDb()
            {
                Id = 3,
                UserId = regularUser.Id,
                Distance = 1,
                Time = 2,
                Date = new DateTime(2020, 1, 29),
                Latitude = 2,
                Longitude = 2
            }
        };
        
        public static void InitializeDbForTests(JoggingTrackerDataContext db, 
            UserManager<UserDb> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in UserRoles.AllRoles)
            {
                IdentityRole role = new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName
                };
                roleManager.CreateAsync(role).Wait();
            }

            foreach (var userDb in SeedUsers)
            {
                userManager.CreateAsync(userDb, UserPassword).Wait();
            }

            userManager.AddToRoleAsync(SeedUsers[0], UserRoles.Admin).Wait();
            userManager.AddToRoleAsync(SeedUsers[1], UserRoles.UserManager).Wait();
            userManager.AddToRoleAsync(SeedUsers[2], UserRoles.RegularUser).Wait();

            db.Runs.AddRange(SeedRegularUserRuns);
            
            db.SaveChanges();
        }
        
        //TODO: remove if reinitialization is not needed
        /*public static void ReinitializeDbForTests(JoggingTrackerDataContext db,
            UserManager<UserDb> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            //TODO: remove roles first
            db.Users.RemoveRange(db.Users);

            InitializeDbForTests(db, userManager, roleManager);
        }*/

        public static DbContextOptions<JoggingTrackerDataContext> GetDbContextOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<JoggingTrackerDataContext>();
            SetInMemoryDbOptions(optionsBuilder);

            return optionsBuilder.Options;
        }

        public static void SetInMemoryDbOptions(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase("InMemoryDbForTesting");
        }
    }
}