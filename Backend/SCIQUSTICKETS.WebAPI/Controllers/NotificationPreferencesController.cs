using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.COMMON.Enums;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationPreferencesController : ControllerBase
	{
		private readonly AppDbContext _context;

		public NotificationPreferencesController(AppDbContext context)
		{
			_context = context;
		}

		// ============================================================
		// DEFAULT TEMPLATE (used when creating new employees, and as
		// the fallback shown for employees with no saved preference)
		// ============================================================

		[HttpGet("default-template")]
		public async Task<IActionResult> GetDefaultTemplate()
		{
			var template = await _context.DefaultEmployeeEmailNotificationPreferences
				.FirstOrDefaultAsync(t => t.Id == 1);

			if (template == null)
			{
				// Should always exist via seeder, but fail safe
				return Ok(new DefaultEmployeeEmailNotificationPreference { Id = 1 });
			}

			return Ok(template);
		}

		public class UpdateDefaultTemplateRequest
		{
			public bool ReceiveAll { get; set; }
			public bool Assignment { get; set; }
			public bool Acceptance { get; set; }
			public bool Rejection { get; set; }
			public bool Expiry { get; set; }
			public bool Reassignment { get; set; }
			public bool StatusChange { get; set; }
			public bool Closure { get; set; }
			public bool Reopen { get; set; }
		}

		[HttpPut("default-template")]
		public async Task<IActionResult> UpdateDefaultTemplate([FromBody] UpdateDefaultTemplateRequest request)
		{
			var template = await _context.DefaultEmployeeEmailNotificationPreferences
				.FirstOrDefaultAsync(t => t.Id == 1);

			if (template == null)
			{
				template = new DefaultEmployeeEmailNotificationPreference { Id = 1 };
				_context.DefaultEmployeeEmailNotificationPreferences.Add(template);
			}

			template.ReceiveAll = request.ReceiveAll;
			template.Assignment = request.Assignment;
			template.Acceptance = request.Acceptance;
			template.Rejection = request.Rejection;
			template.Expiry = request.Expiry;
			template.Reassignment = request.Reassignment;
			template.StatusChange = request.StatusChange;
			template.Closure = request.Closure;
			template.Reopen = request.Reopen;
			template.LastUpdatedDate = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return Ok(template);
		}

		// ============================================================
		// CUSTOMER PREFERENCES (global — same for every customer)
		// ============================================================

		[HttpGet("customer")]
		public async Task<IActionResult> GetCustomerPreferences()
		{
			var prefs = await _context.CustomerNotificationPreferences
				.OrderBy(p => p.Category)
				.ToListAsync();

			return Ok(prefs.Select(p => new { p.Category, p.IsEnabled }));
		}

		public class UpdateCustomerPreferenceRequest
		{
			public bool IsEnabled { get; set; }
		}

		[HttpPut("customer/{category}")]
		public async Task<IActionResult> UpdateCustomerPreference(string category, [FromBody] UpdateCustomerPreferenceRequest request)
		{
			if (!Enum.TryParse<EmailNotificationCategory>(category, true, out var parsed))
				return BadRequest(new { message = $"Unknown category: {category}" });

			var pref = await _context.CustomerNotificationPreferences
				.FirstOrDefaultAsync(p => p.Category == parsed);

			if (pref == null)
			{
				pref = new CustomerNotificationPreference { Category = parsed };
				_context.CustomerNotificationPreferences.Add(pref);
			}

			pref.IsEnabled = request.IsEnabled;
			pref.LastUpdatedDate = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return Ok(new { pref.Category, pref.IsEnabled });
		}

		// ============================================================
		// BULK EMPLOYEE SELECTION BY CATEGORY
		// e.g. "these 20 employees get Assignment emails" — replaces
		// opening 20 individual employee records one at a time.
		// ============================================================

		public class BulkSelectEmployeesRequest
		{
			public List<string> EmployeeIds { get; set; } = new();
		}

		[HttpPut("employees/bulk-by-category/{category}")]
		public async Task<IActionResult> BulkSelectEmployeesForCategory(string category, [FromBody] BulkSelectEmployeesRequest request)
		{
			if (!Enum.TryParse<EmailNotificationCategory>(category, true, out var parsed))
				return BadRequest(new { message = $"Unknown category: {category}" });

			var selectedIds = request.EmployeeIds.Distinct().ToHashSet();

			var allEmployees = await _context.Employees
				.Where(e => !e.IsDeleted)
				.Select(e => e.Id)
				.ToListAsync();

			var existingPreferences = await _context.EmployeeEmailNotificationPreferences
				.Where(p => allEmployees.Contains(p.EmployeeId))
				.ToListAsync();

			var existingByEmployeeId = existingPreferences.ToDictionary(p => p.EmployeeId);

			foreach (var employeeId in allEmployees)
			{
				var shouldReceive = selectedIds.Contains(employeeId);

				if (!existingByEmployeeId.TryGetValue(employeeId, out var pref))
				{
					// No preference row yet — create one, defaulting everything
					// else to false so this bulk action only affects this category.
					pref = new EmployeeEmailNotificationPreference
					{
						EmployeeEmailNotificationPreferenceId = Guid.NewGuid(),
						EmployeeId = employeeId,
						CreatedDate = DateTime.UtcNow
					};
					_context.EmployeeEmailNotificationPreferences.Add(pref);
				}

				SetCategoryFlag(pref, parsed, shouldReceive);
				pref.LastUpdatedDate = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();

			return Ok(new
			{
				category = parsed.ToString(),
				updatedCount = allEmployees.Count,
				selectedCount = selectedIds.Count
			});
		}

		[HttpGet("employees/bulk-by-category/{category}")]
		public async Task<IActionResult> GetSelectedEmployeesForCategory(string category)
		{
			if (!Enum.TryParse<EmailNotificationCategory>(category, true, out var parsed))
				return BadRequest(new { message = $"Unknown category: {category}" });

			var preferences = await _context.EmployeeEmailNotificationPreferences
				.Include(p => p.Employee)
				.Where(p => !p.Employee.IsDeleted)
				.ToListAsync();

			var selectedIds = preferences
				.Where(p => GetCategoryFlag(p, parsed))
				.Select(p => p.EmployeeId)
				.ToList();

			return Ok(new { category = parsed.ToString(), employeeIds = selectedIds });
		}

		private static bool GetCategoryFlag(EmployeeEmailNotificationPreference pref, EmailNotificationCategory category)
		{
			return category switch
			{
				EmailNotificationCategory.Assignment => pref.Assignment,
				EmailNotificationCategory.Acceptance => pref.Acceptance,
				EmailNotificationCategory.Rejection => pref.Rejection,
				EmailNotificationCategory.Expiry => pref.Expiry,
				EmailNotificationCategory.Reassignment => pref.Reassignment,
				EmailNotificationCategory.StatusChange => pref.StatusChange,
				EmailNotificationCategory.Closure => pref.Closure,
				EmailNotificationCategory.Reopen => pref.Reopen,
				_ => false
			};
		}

		private static void SetCategoryFlag(
	EmployeeEmailNotificationPreference pref,
	EmailNotificationCategory category,
	bool value)
		{
			switch (category)
			{
				case EmailNotificationCategory.Assignment: pref.Assignment = value; break;
				case EmailNotificationCategory.Acceptance: pref.Acceptance = value; break;
				case EmailNotificationCategory.Rejection: pref.Rejection = value; break;
				case EmailNotificationCategory.Expiry: pref.Expiry = value; break;
				case EmailNotificationCategory.Reassignment: pref.Reassignment = value; break;
				case EmailNotificationCategory.StatusChange: pref.StatusChange = value; break;
				case EmailNotificationCategory.Closure: pref.Closure = value; break;
				case EmailNotificationCategory.Reopen: pref.Reopen = value; break;
			}
		}
	}
}