using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dating_WebAPI.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ActionExecutedContext resultContext = await next();

            // 如果沒有驗證，就不做事。
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            // 我們有實作ClaimsPrincipalExtension，所以可以找到GetUserName()
            int userId = resultContext.HttpContext.User.GetUserId();
            IUserRepository repository = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            var user = await repository.GetUserByIdAsync(userId);
            user.LastLoginTime = DateTime.Now;
            await repository.SaveAllAsync();
        }
    }
}