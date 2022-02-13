using Dating_WebAPI.Data;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Interfaces;
using Dating_WebAPI.Middleware;
using Dating_WebAPI.Services;
using Dating_WebAPI.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dating_WebAPI
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 為了更好的去維護及區分各服務，可以再額外建一個Extension的Class來使用。
            services.AddApplicationServices(_configuration);
            services.AddIdentityServices(_configuration);
            services.AddSignalR();
            services.AddControllers();

            // 啟用相同原始來源原則，在Configure定義來源。
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Middleware寫在這裡
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            // 啟用相同原始來源原則，正確寫法是寫在這裡。
            app.UseCors(policy => policy.AllowAnyHeader()
                                        .AllowAnyMethod()
                                        // 告知瀏覽器伺服器允許跨原始來源要求的認證, signalR要求我們設定此項
                                        // 原本是用來接收透過cookie過來的認證
                                        .AllowCredentials()
                                        .WithOrigins("https://localhost:4200"));

            // 要在UseAuthorization()之前，先驗證、再授權。
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // 設定signalR的路由
                endpoints.MapHub<PresenceHub>("hubs/presence");
                endpoints.MapHub<MessageHub>("hubs/message");

                endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }
}