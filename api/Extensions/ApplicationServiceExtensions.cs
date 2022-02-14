using System;
using AutoMapper;
using Dating_WebAPI.Data;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Dating_WebAPI.Services;
using Dating_WebAPI.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dating_WebAPI.Extensions
{
    public static class ApplicationServiceExtensions
    {
        // this 用於擴展方法中，用來指定該方法作用於哪個類型，
        // 擴展方法應為靜態
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 將appsettings.json的資料抓過來變成強行別物件。
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            services.AddScoped<IPhotosServices, PhotoServices>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 使用IAsyncActionFilter偵測每一次呼叫API時，可以做什麼事情。
            services.AddScoped<LogUserActivity>();

            //設定LifeTime只存在一個request，在同一個Requset中，不論是在哪邊被注入，都是同樣的實例。
            services.AddScoped<ITokenServices, TokenServices>();

            // 加入signalR的追蹤
            services.AddSingleton<PresenceTracker>();

            //加入AutoMapper，參數內帶入Profiles的位址，這裡我們只創建一個Profiles
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.AddDbContext<DataContext>(option =>
            {

                // these are for keroku connection
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                string connStr;

                // Depending on if in development or production, use either Heroku-provided
                // connection string, or development connection string from env var.
                if (env == "Development")
                {
                    // Use connection string from file.
                    connStr = configuration.GetConnectionString("DefaultConnection");
                }
                else
                {
                    // Use connection string provided at runtime by Heroku.
                    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                    // Parse connection URL to connection string for Npgsql
                    connUrl = connUrl.Replace("postgres://", string.Empty);
                    var pgUserPass = connUrl.Split("@")[0];
                    var pgHostPortDb = connUrl.Split("@")[1];
                    var pgHostPort = pgHostPortDb.Split("/")[0];
                    var pgDb = pgHostPortDb.Split("/")[1];
                    var pgUser = pgUserPass.Split(":")[0];
                    var pgPass = pgUserPass.Split(":")[1];
                    var pgHost = pgHostPort.Split(":")[0];
                    var pgPort = pgHostPort.Split(":")[1];

                    connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;TrustServerCertificate=True";
                }

                // Whether the connection string came from the local development configuration file
                // or from the environment variable from Heroku, use it to set up your DbContext.
                option.UseNpgsql(connStr);


                //option.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

                // 在appsettings.json的LogLevel中加入"Microsoft.EntityFrameworkCore.Database.Command": "Information"
                // 來開啟紀錄EF core 的查詢語法，因其查詢的參數值預設是編碼過後的(個資法)，因此使用EnableSensitiveDataLogging()
                // 使其可以看到傳入的參數
                option.EnableSensitiveDataLogging();

                option.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Name }, LogLevel.Information);
            });

            return services;
        }
    }
}