using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BBB.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole _requiredRole;

    public RequireRoleAttribute(UserRole role)
    {
        _requiredRole = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var userIdString = context.HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            context.Result = new ContentResult
            {
                Content = "Please log in to access this resource.",
                StatusCode = 401,
                ContentType = "text/plain"
            };
            return;
        }

        var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            context.Result = new ContentResult
            {
                Content = "Your session has expired. Please log in again.",
                StatusCode = 401,
                ContentType = "text/plain"
            };
            return;
        }

        if (user.RoleId != (int)_requiredRole)
        {
            context.Result = new ContentResult
            {
                Content = "You don't have permission to access this resource.",
                StatusCode = 403,
                ContentType = "text/plain"
            };
            return;
        }
    }
}
