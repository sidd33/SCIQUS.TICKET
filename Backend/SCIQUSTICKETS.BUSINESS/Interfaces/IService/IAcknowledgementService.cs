using System;
using System.Threading.Tasks;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IAcknowledgementService
	{
		Task HandleAsync(
			Guid ticketId,
			string eventType,
			string? actorUserId);
	}
}