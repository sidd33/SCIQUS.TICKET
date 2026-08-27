using System;
using System.Threading.Tasks;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IEmployeeEmailNotificationService
	{
		Task SendTicketNotificationAsync(
			Guid ticketId,
			string eventType,
			string? actorUserId = null,
			string? remarks = null);
	}
}