using System;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultcontext = await next();

        if (resultcontext.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var userId = resultcontext.HttpContext.User.GetUserId();

        var repo = resultcontext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByUserIdAsync(userId);
        if (user == null) return;
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
