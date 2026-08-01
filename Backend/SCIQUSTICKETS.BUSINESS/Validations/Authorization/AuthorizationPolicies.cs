using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace SCIQUSTICKETS.BUSINESS.Validations.Authorization
{
    /// <summary>
    /// Requirement: The current user must be the resource owner OR have the Admin role.
    /// Usage: Apply [Authorize(Policy = "SameUserOrAdmin")] on controllers/actions.
    /// </summary>
    public class SameUserOrAdminRequirement : IAuthorizationRequirement { }

    public class SameUserOrAdminHandler : AuthorizationHandler<SameUserOrAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameUserOrAdminRequirement requirement)
        {
            var currentUserId = context.User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Admins always pass
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Resource owner check — resource must expose the owner's userId as a string
            if (context.Resource is string resourceOwnerId &&
                currentUserId == resourceOwnerId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Central place to register all custom authorization policies.
    /// Call this from Program.cs: builder.Services.AddAuthorizationPolicies();
    /// </summary>
    public static class AuthorizationPolicies
    {
        public static void AddAuthorizationPolicies(
            this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SameUserOrAdmin", policy =>
                    policy.Requirements.Add(new SameUserOrAdminRequirement()));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });

            services.AddSingleton<
                IAuthorizationHandler,
                SameUserOrAdminHandler>();
        }
    }
}
