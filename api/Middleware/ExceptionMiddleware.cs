using Dating_WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dating_WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        // ASP.NET Core 在 Middleware 的官方說明中，使用了 Pipeline 這個名詞，意旨 Middleware 像水管一樣可以串聯在一起，所有的 Request 及 Response 都會層層經過這些水管。
        // 在 Pipeline 的概念中，註冊順序是很重要的事情。資料經過的順序一定是先進後出。
        // RequestDelegate代表在Middleware的下一步要做什麼，ILogger使我們在發生錯誤時，記錄錯誤資訊。
        // IHostEnvironment代表是處於開發模式，還是產品模式。
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // 因為是針對HttpRequest進來的Middelware，所以參數丟上HttpContext
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 傳到下個Middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // 可以在Terminal顯示Exception
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                // Http 500
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment() ?
                    new APIException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) :
                    new APIException(context.Response.StatusCode, "Internal Server Error");

                var options = new JsonSerializerOptions
                {
                    // 使Json的formatted是CamelCase
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}