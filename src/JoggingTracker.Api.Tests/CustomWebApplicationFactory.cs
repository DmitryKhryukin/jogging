using System;
using System.Linq;
using JoggingTracker.DataAccess;
using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JoggingTracker.Api.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's db context registration.
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<JoggingTrackerDataContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add db context using an in-memory database for testing.
                services.AddDbContext<JoggingTrackerDataContext>(FakeDbUtilities.SetInMemoryDbOptions);

                // Build the service provider.
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<JoggingTrackerDataContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<UserDb>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                // Ensure the database is created.
                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data.
                    FakeDbUtilities.InitializeDbForTests(db, userManager, roleManager);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred seeding the database with test messages. Error: {ex.Message}" );
                }
            });
        }
    }
}