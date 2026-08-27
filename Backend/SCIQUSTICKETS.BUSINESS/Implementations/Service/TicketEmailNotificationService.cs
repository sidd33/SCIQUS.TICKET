using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class TicketEmailNotificationService : ITicketEmailNotificationService
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly ILogger<TicketEmailNotificationService> _logger;

		public TicketEmailNotificationService(
			AppDbContext context,
			IConfiguration configuration,
			ILogger<TicketEmailNotificationService> logger)
		{
			_context = context;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendCustomerStatusEmailAsync(
			Guid ticketId,
			string statusName)
		{
			var ticket = await _context.Tickets
				.Include(t => t.Account)
				.ThenInclude(a => a.Contacts)
				.FirstOrDefaultAsync(t => t.TicketId == ticketId);

			if (ticket == null)
			{
				_logger.LogWarning(
					"Cannot send status email. Ticket {TicketId} was not found.",
					ticketId);

				return;
			}

			if (ticket.Account == null)
			{
				_logger.LogWarning(
					"Cannot send status email. Ticket {TicketId} has no account.",
					ticketId);

				return;
			}

			var recipientEmail = !string.IsNullOrWhiteSpace(ticket.Account.Email)
			? ticket.Account.Email
			: ticket.Account.Contacts
				.Where(c => !string.IsNullOrWhiteSpace(c.Email))
				.Select(c => c.Email)
				.FirstOrDefault();

			if (string.IsNullOrWhiteSpace(recipientEmail))
			{
				_logger.LogWarning(
					"Cannot send status email. No customer email found for ticket {TicketId}.",
					ticketId);

				return;
			}

			var ticketNumber = ticket.TicketNumber ?? ticket.TicketId.ToString();

			var subject = $"Ticket {ticketNumber} status updated to {statusName}";

			var body = BuildCustomerStatusEmail(
				ticketNumber,
				ticket.Title,
				statusName);

			try
			{
				await SendEmailAsync(
					recipientEmail,
					subject,
					body);

				_logger.LogInformation(
					"Customer status email sent for ticket {TicketId} to {Email}. Status: {Status}",
					ticketId,
					recipientEmail,
					statusName);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to send customer status email for ticket {TicketId}.",
					ticketId);

				// Important:
				// Do not fail the ticket status operation just because
				// the acknowledgement email failed.
			}
		}

		private async Task SendEmailAsync(
			string recipientEmail,
			string subject,
			string body)
		{
			var host = _configuration["EmailNotification:SmtpHost"];
			var portString = _configuration["EmailNotification:SmtpPort"];
			var username = _configuration["EmailNotification:Username"];
			var password = _configuration["EmailNotification:Password"];
			var fromEmail = _configuration["EmailNotification:FromEmail"];
			var enableSslString = _configuration["EmailNotification:EnableSsl"];

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
				smtp.Credentials = new NetworkCredential(
					username,
					password);
			}

			await smtp.SendMailAsync(message);
		}

		private static string BuildCustomerStatusEmail(
			string ticketNumber,
			string title,
			string statusName)
		{
			return $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family: Arial, sans-serif;">

                    <h2>Ticket Status Update</h2>

                    <p>Hello,</p>

                    <p>
                        Your support ticket has been updated.
                    </p>

                    <table cellpadding="8" cellspacing="0">
                        <tr>
                            <td><strong>Ticket</strong></td>
                            <td>{WebUtility.HtmlEncode(ticketNumber)}</td>
                        </tr>

                        <tr>
                            <td><strong>Subject</strong></td>
                            <td>{WebUtility.HtmlEncode(title)}</td>
                        </tr>

                        <tr>
                            <td><strong>Status</strong></td>
                            <td><strong>{WebUtility.HtmlEncode(statusName)}</strong></td>
                        </tr>
                    </table>

                    <p>
                        You will receive another notification when
                        the ticket status changes.
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