using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helper;

public class UpdateUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = resultContext.HttpContext.User.GetUserId();
        var repository = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return;
        }

        user.LastActive = DateTime.UtcNow;

        await repository.SaveAllAsync();
    }
}
