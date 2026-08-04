using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
	public class TicketAttachmentRepository : GenericRepository<TicketAttachment>, ITicketAttachmentRepository
	{
		public TicketAttachmentRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<TicketAttachment>> GetByTicketIdAsync(Guid ticketId)
			=> await _dbSet
				.Include(a => a.UploadedByUser)
				.Where(a => a.TicketId == ticketId && !a.IsDeleted)
				.OrderByDescending(a => a.UploadedDate)
				.ToListAsync();

		public async Task<TicketAttachment?> GetByIdForTicketAsync(Guid ticketId, Guid attachmentId)
			=> await _dbSet
				.Include(a => a.UploadedByUser)
				.FirstOrDefaultAsync(a => a.TicketId == ticketId && a.TicketAttachmentId == attachmentId && !a.IsDeleted);
	}
}