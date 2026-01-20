using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RepositoryPatternWithUnitOfWork.EF;
using System.Security.Claims;

namespace E_Commerce2.Authorization
{
    public class PermissionBasedAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requiredRoles = context.ActionDescriptor.EndpointMetadata
                .OfType<CheckPermissionAttribute>()
                .Select(a => a.Role.ToString())
                .ToList();

            if (!requiredRoles.Any())
                return;

            var user = context.HttpContext.User;

            if (!user.Identity!.IsAuthenticated)
            {
                context.Result = new ForbidResult();
                return;
            }

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var hasPermission = userRoles.Any(r => requiredRoles.Contains(r));

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
