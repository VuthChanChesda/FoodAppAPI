using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class AuthorizeOwnerAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdFromToken = int.Parse(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        if (!context.ActionArguments.TryGetValue("id", out var idObj) || (int)idObj != userIdFromToken)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
