using FoodAppAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeOwnerAttribute : Attribute, IAsyncActionFilter
{
    private readonly Type _entityType;
    private readonly string _foreignKeyName;

    public AuthorizeOwnerAttribute(Type entityType, string foreignKeyName)
    {
        _entityType = entityType;
        _foreignKeyName = foreignKeyName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1️⃣ Get the logged-in user ID from JWT
        var userIdFromToken = int.Parse(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // 2️⃣ Get the "id" route parameter
        if (!context.ActionArguments.TryGetValue("id", out var idObj))
        {
            context.Result = new ForbidResult();
            return;
        }

        var entityId = (int)idObj;

        // 3️⃣ Get DbContext from DI
        var db = context.HttpContext.RequestServices.GetService(typeof(foodAppContext)) as foodAppContext;
        if (db == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // 4️⃣ Find the entity dynamically by type and primary key
        var entity = await db.FindAsync(_entityType, entityId);
        if (entity == null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        // 5️⃣ Get the foreign key property via reflection
        var fkProperty = _entityType.GetProperty(_foreignKeyName);
        if (fkProperty == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // 6️⃣ Check if the logged-in user is the owner
        var ownerId = (int)fkProperty.GetValue(entity)!;
        if (ownerId != userIdFromToken)
        {
            context.Result = new ForbidResult();
            return;
        }

        // 7️⃣ Continue to the action
        await next();
    }
}
