using Dating_WebAPI.Data;
using Dating_WebAPI.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Asp.net core?????DHttp Request??services?s?b?A?i????CreateScope?????C
            // ?b?o???N?O?bMiddleware???~?F?A????Middleware???????C
            using var scope = host.Services.CreateScope();
            var service = scope.ServiceProvider;
            try
            {
                // ?C?????????????A?i?H?i???????w??Migration?C
                var context = service.GetRequiredService<DataContext>();
                var userManager = service.GetRequiredService<UserManager<AppUser>>();
                var roleManager = service.GetRequiredService<RoleManager<AppRole>>();
                await context.Database.MigrateAsync();
                await Seed.SeedUsers(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = service.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occur during migration!");
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}