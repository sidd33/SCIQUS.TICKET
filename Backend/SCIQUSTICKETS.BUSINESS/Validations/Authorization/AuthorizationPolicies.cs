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

			// Admins, SuperAdmins, and DepartmentHeads always pass
			if (context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin") || context.User.IsInRole("DepartmentHead"))
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
	///
	/// Policy names below are sourced directly from the SCIQUS AMS Ticketing
	/// System requirements doc (one section per module). Role names in
	/// RequireRole(...) are ASSUMPTIONS — confirm against the real role seed
	/// data / UserRole table before relying on this in production.
	/// </summary>
	public static class AuthorizationPolicies
	{
		public static void AddAuthorizationPolicies(
			this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
		{
			services.AddAuthorization(options =>
			{
				// ── Generic / existing ────────────────────────────────
				options.AddPolicy("SameUserOrAdmin", policy =>
					policy.Requirements.Add(new SameUserOrAdminRequirement()));

				options.AddPolicy("AdminOnly", policy =>
					policy.RequireRole("Admin"));

				// ── Module 1: Ticket Master Data & Configuration ──────
				options.AddPolicy("ticketmaster.view", policy =>
    policy.RequireRole("Employee", "Customer", "Admin", "SuperAdmin"));

				options.AddPolicy("ticketmaster.manage", policy =>
					policy.RequireRole("Admin", "SuperAdmin"));

				// ── Module 2: Core Ticket Lifecycle ───────────────────
				options.AddPolicy("ticket.view", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));

				options.AddPolicy("ticket.create", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));

				options.AddPolicy("ticket.manage", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));

				options.AddPolicy("ticket.manage.all", policy =>
					policy.RequireRole("Manager", "Admin", "SuperAdmin"));

				options.AddPolicy("ticket.delete", policy =>
					policy.RequireRole("Manager", "Admin", "SuperAdmin"));

				// ── Module 3: Assignment & Routing ────────────────────
				options.AddPolicy("ticket.assign", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));

				options.AddPolicy("ticket.transfer", policy =>
					policy.RequireRole("Manager", "Admin", "SuperAdmin"));

				// ── Module 6: Email Channel ────────────────────────────
				options.AddPolicy("emailticket.config", policy =>
					policy.RequireRole("Admin", "SuperAdmin"));

				options.AddPolicy("emailticket.review", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));

				// ── Module 7: Customer / Account Portal ───────────────
				options.AddPolicy("portalticket.access", policy =>
					policy.RequireRole("Customer", "Admin", "SuperAdmin"));

				// ── Module 8: WhatsApp Channel ────────────────────────
				options.AddPolicy("whatsapp.config", policy =>
					policy.RequireRole("Admin", "SuperAdmin"));

				options.AddPolicy("whatsapp.review", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));

				// ── Module 10: Reporting & Dashboard ──────────────────
				options.AddPolicy("ticket.report.view", policy =>
					policy.RequireRole("DepartmentHead", "Employee", "Agent", "Manager", "Admin", "SuperAdmin"));

				options.AddPolicy("ticket.report.all", policy =>
					policy.RequireRole("Admin", "SuperAdmin"));

				// ── Module 13: Support Plans & Ticket Entitlement ─────
				options.AddPolicy("supportplan.manage", policy =>
					policy.RequireRole("Admin", "SuperAdmin"));

				options.AddPolicy("supportplan.view", policy =>
					policy.RequireRole("Agent", "Manager", "Admin", "SuperAdmin"));
			});

			services.AddSingleton<IAuthorizationHandler, SameUserOrAdminHandler>();
		}
	}
}