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

            services.AddScoped<IMessageRepository, MessageRepository>();

            // 使用IAsyncActionFilter偵測每一次呼叫API時，可以做什麼事情。
            services.AddScoped<LogUserActivity>();

            services.AddScoped<ILikesRepository, LikesRepository>();

            //設定LifeTime只存在一個request，在同一個Requset中，不論是在哪邊被注入，都是同樣的實例。
            services.AddScoped<ITokenServices, TokenServices>();

            //加入UserRepository
            services.AddScoped<IUserRepository, UseRepository>();

            // 加入signalR的追蹤
            services.AddSingleton<PresenceTracker>();

            //加入AutoMapper，參數內帶入Profiles的位址，這裡我們只創建一個Profiles
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.AddDbContext<DataContext>(option =>
            {
                option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

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