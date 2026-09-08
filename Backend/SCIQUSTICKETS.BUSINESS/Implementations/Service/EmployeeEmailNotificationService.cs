using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class EmployeeEmailNotificationService : IEmployeeEmailNotificationService
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmployeeEmailNotificationService> _logger;

		public EmployeeEmailNotificationService(
			AppDbContext context,
			IConfiguration configuration,
			ILogger<EmployeeEmailNotificationService> logger)
		{
			_context = context;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendTicketNotificationAsync(
			Guid ticketId,
			string eventType,
			string? actorUserId = null,
			string? remarks = null)
		{
			var ticket = await _context.Tickets
				.Include(t => t.Account)
				.Include(t => t.TicketType)
				.Include(t => t.TicketSubType)
				.Include(t => t.Priority)
				.Include(t => t.Status)
				.Include(t => t.AssignedToUser)
				.FirstOrDefaultAsync(t => t.TicketId == ticketId);

			if (ticket == null)
			{
				_logger.LogWarning(
					"Cannot send employee notification. Ticket {TicketId} was not found.",
					ticketId);

				return;
			}

			var ticketNumber =
				ticket.TicketNumber ?? ticket.TicketId.ToString();

			var subject =
				$"Ticket {ticketNumber} - {GetEventTitle(eventType)}";

			var body = BuildEmployeeEmail(
				ticketNumber,
				ticket.Title,
				ticket.Status?.Name ?? "Unknown",
				ticket.Priority?.Name,
				ticket.TicketType?.Name,
				ticket.TicketSubType?.Name,
				eventType,
				remarks);

			// Get all employees who have enabled this event type
			// in their notification preferences.
			var recipientEmails =
				await GetRecipientEmailsAsync(eventType);

			if (recipientEmails.Count == 0)
			{
				_logger.LogInformation(
					"No employees are configured to receive email event {EventType} for ticket {TicketId}.",
					eventType,
					ticketId);

				return;
			}

			foreach (var recipientEmail in recipientEmails)
			{
				try
				{
					await SendEmailAsync(
						recipientEmail,
						subject,
						body);

					_logger.LogInformation(
						"Employee notification sent for ticket {TicketId} to {Email}. Event: {EventType}",
						ticketId,
						recipientEmail,
						eventType);
				}
				catch (Exception ex)
				{
					_logger.LogError(
						ex,
						"Failed to send employee notification for ticket {TicketId} to {Email}. Event: {EventType}",
						ticketId,
						recipientEmail,
						eventType);

					// Email failure for one employee must not
					// prevent notifications to other employees.
				}
			}
		}


		public async Task SendManagerEscalationNotificationAsync(Guid ticketId)
		{
			var ticket = await _context.Tickets
				.Include(t => t.Department)
				.Include(t => t.TicketType)
				.Include(t => t.TicketSubType)
				.Include(t => t.Priority)
				.Include(t => t.Status)
				.FirstOrDefaultAsync(t => t.TicketId == ticketId);

			if (ticket == null)
			{
				_logger.LogWarning(
					"Cannot send manager escalation notification. Ticket {TicketId} was not found.",
					ticketId);

				return;
			}

			if (ticket.Department == null ||
				string.IsNullOrWhiteSpace(ticket.Department.DepartmentHeadId))
			{
				_logger.LogWarning(
					"Cannot send manager escalation notification for ticket {TicketId}. Department manager is not configured.",
					ticketId);

				return;
			}

			var manager = await _context.Employees
				.FirstOrDefaultAsync(e =>
					e.Id == ticket.Department.DepartmentHeadId &&
					!e.IsDeleted &&
					!string.IsNullOrWhiteSpace(e.Email));

			if (manager == null)
			{
				_logger.LogWarning(
					"Cannot send manager escalation notification for ticket {TicketId}. Department manager was not found or has no email.",
					ticketId);

				return;
			}

			var ticketNumber =
				ticket.TicketNumber ?? ticket.TicketId.ToString();

			var subject =
				$"Ticket {ticketNumber} - Manager Escalation";

			var body = BuildManagerEscalationEmail(
				ticketNumber,
				ticket.Title,
				ticket.Status?.Name ?? "Unknown",
				ticket.Priority?.Name,
				ticket.TicketType?.Name,
				ticket.TicketSubType?.Name);

			try
			{
				await SendEmailAsync(
					manager.Email,
					subject,
					body);

				_logger.LogInformation(
					"Manager escalation notification sent for ticket {TicketId} to {Email}.",
					ticketId,
					manager.Email);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to send manager escalation notification for ticket {TicketId} to {Email}.",
					ticketId,
					manager.Email);
			}
		}

		private async Task SendEmailAsync(
			string recipientEmail,
			string subject,
			string body)
		{
			var host =
				_configuration["EmailNotification:SmtpHost"];

			var portString =
				_configuration["EmailNotification:SmtpPort"];

			var username =
				_configuration["EmailNotification:Username"];

			var password =
				_configuration["EmailNotification:Password"];

			var fromEmail =
				_configuration["EmailNotification:FromEmail"];

			var enableSslString =
				_configuration["EmailNotification:EnableSsl"];

			if (string.IsNullOrWhiteSpace(host))
				throw new InvalidOperationException(
					"EmailNotification:SmtpHost is not configured.");

			if (!int.TryParse(portString, out var port))
				port = 587;

			bool enableSsl =
				!string.Equals(
					enableSslString,
					"false",
					StringComparison.OrdinalIgnoreCase);

			if (string.IsNullOrWhiteSpace(fromEmail))
				fromEmail = username;

			if (string.IsNullOrWhiteSpace(fromEmail))
				throw new InvalidOperationException(
					"EmailNotification:FromEmail is not configured.");

			using var message = new MailMessage();

			message.From = new MailAddress(fromEmail);
			message.To.Add(recipientEmail);
			message.Subject = subject;
			message.Body = body;
			message.IsBodyHtml = true;

			using var smtp = new SmtpClient(host, port);

			smtp.EnableSsl = enableSsl;

			if (!string.IsNullOrWhiteSpace(username))
			{
				smtp.Credentials =
					new NetworkCredential(username, password);
			}

			await smtp.SendMailAsync(message);
		}

		private static string GetEventTitle(string eventType)
		{
			return eventType switch
			{
				"TicketCreated" => "New Ticket Created",
				"Assigned" => "Ticket Assigned",
				"Reassigned" => "Ticket Reassigned",
				"Transferred" => "Ticket Transferred",
				"AcceptancePending" => "Acceptance Required",
				"Accepted" => "Ticket Accepted",
				"Rejected" => "Ticket Rejected",
				"AcceptanceExpired" => "Acceptance Expired",
				"FallbackAssigned" => "Ticket Reassigned",
				"InProgress" => "Ticket Accepted / In Progress",
				"Pending" => "Ticket Pending",
				"PendingClosure" => "Pending Closure",
				"Closed" => "Ticket Closed",
				"Reopened" => "Ticket Reopened",
				"CommentAdded" => "New Comment Added",
				"PriorityChanged" => "Ticket Priority Changed",
				"DepartmentTransferred" => "Ticket Department Transferred",
				_ => "Ticket Update"
			};
		}

		private async Task<List<string>> GetRecipientEmailsAsync(string eventType)
		{
			var preferences = await _context.EmployeeEmailNotificationPreferences
				.Include(p => p.Employee)
				.Where(p =>
					!p.Employee.IsDeleted &&
					!string.IsNullOrWhiteSpace(p.Employee.Email))
				.ToListAsync();

			var recipients = new List<string>();

			foreach (var preference in preferences)
			{
				if (ShouldReceive(preference, eventType))
				{
					recipients.Add(preference.Employee.Email);
				}
			}

			return recipients
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private static bool ShouldReceive(
	EmployeeEmailNotificationPreference preference,
	string eventType)
		{
			if (preference.ReceiveAll)
				return true;

			return eventType switch
			{
				"TicketCreated" => preference.Assignment,
				"Assigned" => preference.Assignment,
				"Transferred" => preference.Assignment,

				"AcceptancePending" => preference.Acceptance,
				"Accepted" => preference.Acceptance,

				"Rejected" => preference.Rejection,

				"AcceptanceExpired" => preference.Expiry,

				"Reassigned" => preference.Reassignment,
				"FallbackAssigned" => preference.Reassignment,

				"InProgress" => preference.StatusChange,
				"Pending" => preference.StatusChange,
				"PriorityChanged" => preference.StatusChange,
				"DepartmentTransferred" => preference.StatusChange,

				"PendingClosure" => preference.Closure,
				"Closed" => preference.Closure,

				"Reopened" => preference.Reopen,

				_ => false
			};
		}


		private static string BuildEmployeeEmail(
			string ticketNumber,
			string title,
			string status,
			string? priority,
			string? ticketType,
			string? ticketSubType,
			string eventType,
			string? remarks)
		{
			var safeTicketNumber =
				WebUtility.HtmlEncode(ticketNumber);

			var safeTitle =
				WebUtility.HtmlEncode(title);

			var safeStatus =
				WebUtility.HtmlEncode(status);

			var safePriority =
				WebUtility.HtmlEncode(priority ?? "N/A");

			var safeType =
				WebUtility.HtmlEncode(ticketType ?? "N/A");

			var safeSubType =
				WebUtility.HtmlEncode(ticketSubType ?? "N/A");

			var safeRemarks =
				WebUtility.HtmlEncode(remarks ?? "");



			return $"""
				<!DOCTYPE html>
				<html>
				<body style="font-family: Arial, sans-serif;">

					<h2>Ticket Notification</h2>

					<p>Hello,</p>

					<p>
						There has been an update regarding a ticket assigned
						to you.
					</p>

					<table cellpadding="8" cellspacing="0">
						<tr>
							<td><strong>Ticket</strong></td>
							<td>{safeTicketNumber}</td>
						</tr>

						<tr>
							<td><strong>Subject</strong></td>
							<td>{safeTitle}</td>
						</tr>

						<tr>
							<td><strong>Ticket Type</strong></td>
							<td>{safeType}</td>
						</tr>

						<tr>
							<td><strong>Sub-Type</strong></td>
							<td>{safeSubType}</td>
						</tr>

						<tr>
							<td><strong>Priority</strong></td>
							<td>{safePriority}</td>
						</tr>

						<tr>
							<td><strong>Current Status</strong></td>
							<td><strong>{safeStatus}</strong></td>
						</tr>

						<tr>
							<td><strong>Event</strong></td>
							<td>{WebUtility.HtmlEncode(eventType)}</td>
						</tr>
					</table>

					{(string.IsNullOrWhiteSpace(safeRemarks)
						? ""
						: $"<p><strong>Remarks:</strong> {safeRemarks}</p>")}

					<p>
						Please log in to SCIQUS Ticketing System
						to view the ticket and take any required action.
					</p>

					<p>
						Regards,<br/>
						SCIQUS Support Team
					</p>

				</body>
				</html>
				""";
		}


		private static string BuildManagerEscalationEmail(
	string ticketNumber,
	string title,
	string status,
	string? priority,
	string? ticketType,
	string? ticketSubType)
		{
			var safeTicketNumber =
				WebUtility.HtmlEncode(ticketNumber);

			var safeTitle =
				WebUtility.HtmlEncode(title);

			var safeStatus =
				WebUtility.HtmlEncode(status);

			var safePriority =
				WebUtility.HtmlEncode(priority ?? "N/A");

			var safeType =
				WebUtility.HtmlEncode(ticketType ?? "N/A");

			var safeSubType =
				WebUtility.HtmlEncode(ticketSubType ?? "N/A");

			return $"""
		<!DOCTYPE html>
		<html>
		<body style="font-family: Arial, sans-serif;">

			<h2>Ticket Manager Escalation</h2>

			<p>Hello,</p>

			<p>
				The following ticket has been escalated to you because
				no eligible employee was available to accept the ticket
				after the configured fallback attempts.
			</p>

			<table cellpadding="8" cellspacing="0">
				<tr>
					<td><strong>Ticket</strong></td>
					<td>{safeTicketNumber}</td>
				</tr>

				<tr>
					<td><strong>Subject</strong></td>
					<td>{safeTitle}</td>
				</tr>

				<tr>
					<td><strong>Ticket Type</strong></td>
					<td>{safeType}</td>
				</tr>

				<tr>
					<td><strong>Sub-Type</strong></td>
					<td>{safeSubType}</td>
				</tr>

				<tr>
					<td><strong>Priority</strong></td>
					<td>{safePriority}</td>
				</tr>

				<tr>
					<td><strong>Current Status</strong></td>
					<td><strong>{safeStatus}</strong></td>
				</tr>
			</table>

			<p>
				Please review this ticket and take the required action.
			</p>

			<p>
				Regards,<br/>
				SCIQUS Support Team
			</p>

		</body>
		</html>
		""";
		}
	}
}