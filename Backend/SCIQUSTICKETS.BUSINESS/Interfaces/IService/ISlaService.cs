// SCIQUSTICKETS.BUSINESS/Interfaces/IService/ISlaService.cs
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface ISlaService
	{
		DateTime GetClockStart(Ticket ticket);
		(DateTime? SlaDueDate, string? SlaMetStatus) ComputeSlaDueDate(Ticket ticket, TicketPriority? priority);
		void FinalizeOnClose(Ticket ticket, string closureConfirmedBy);
		void ApplyReopen(Ticket ticket, TicketPriority? priority);
		Task ProcessBreachesAsync();
		Task ProcessAutoClosuresAsync();
	}
}