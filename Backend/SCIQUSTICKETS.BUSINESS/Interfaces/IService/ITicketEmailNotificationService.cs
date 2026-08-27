using System;
using System.Threading.Tasks;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface ITicketEmailNotificationService
	{
		Task SendCustomerStatusEmailAsync(
			Guid ticketId,
			string statusName);
	}
}