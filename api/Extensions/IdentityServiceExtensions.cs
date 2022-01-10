using Dating_WebAPI.Data;
using Dating_WebAPI.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dating_WebAPI.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityCore<AppUser>(opt =>
            {
                // 設定密碼強度
                opt.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<AppRole>()
            // 一定要包在RoleManager裡面不然會噴500。
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"])),
                    // API
                    ValidateIssuer = false,
                    // Angular
                    ValidateAudience = false,
                    // 驗證 Token 的有效期間
                    ValidateLifetime = true
                };

                // Because signalR or WebSocket cannot send an authentication header
                // This cant allow our client sending the token as query string
                options.Events = new JwtBearerEvents
                {
                    // Func<T, TResult>
                    OnMessageReceived = context =>
                    {
                        // signalR send token by default key "access_token"
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        // 可能會有很多個hub，所以寫到/hubs就好
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });


            // 基於AdminController設定權限
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }

        private static int SignInManager<T>()
        {
            throw new NotImplementedException();
        }
    }
}