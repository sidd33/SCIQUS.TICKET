using System;
using System.Collections.Generic;
using System.Text;

using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.DATA.Interfaces.IRepositories
{
	public interface ITicketAttachmentRepository : IGenericRepository<TicketAttachment>
	{
		Task<IEnumerable<TicketAttachment>> GetByTicketIdAsync(Guid ticketId);
		Task<TicketAttachment?> GetByIdForTicketAsync(Guid ticketId, Guid attachmentId);
	}
}
