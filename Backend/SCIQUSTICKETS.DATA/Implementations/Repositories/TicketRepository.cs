using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.DATA.Implementations.Repositories
{
	public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
	{
		public TicketRepository(AppDbContext context) : base(context) { }

		public async Task<(IEnumerable<Ticket> Items, int TotalCount)> GetAllPagedAsync(
			string? search,
			Guid? ticketTypeId,
			Guid? ticketSubTypeId,
			Guid? priorityId,
			Guid? businessImpactId,
			Guid? statusId,
			string? accountId,
			string? assignedToUserId,
			bool? isInternal,
			bool? isOpen,
			DateTime? fromDate,
			DateTime? toDate,
			string? sortBy,
			bool sortDescending,
			int page,
			int pageSize)
		{
			var query = _dbSet
				.Include(t => t.TicketType)
				.Include(t => t.TicketSubType)
				.Include(t => t.Priority)
				.Include(t => t.BusinessImpact)
				.Include(t => t.Status)
				.Include(t => t.Account)
				.Include(t => t.CreatedByUser)
				.Include(t => t.AssignedToUser)
				.Include(t => t.RaisedByEmployee)
				.Where(t => !t.IsDeleted)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(t => t.TicketNumber.Contains(search) || t.Title.Contains(search));

			if (ticketTypeId.HasValue) query = query.Where(t => t.TicketTypeId == ticketTypeId);
			if (ticketSubTypeId.HasValue) query = query.Where(t => t.TicketSubTypeId == ticketSubTypeId);
			if (priorityId.HasValue) query = query.Where(t => t.PriorityId == priorityId);
			if (businessImpactId.HasValue) query = query.Where(t => t.BusinessImpactId == businessImpactId);
			if (statusId.HasValue) query = query.Where(t => t.StatusId == statusId);
			if (!string.IsNullOrWhiteSpace(accountId)) query = query.Where(t => t.AccountId == accountId);
			if (!string.IsNullOrWhiteSpace(assignedToUserId)) query = query.Where(t => t.AssignedToUserId == assignedToUserId);
			if (isInternal.HasValue) query = query.Where(t => t.IsInternal == isInternal);
			if (isOpen.HasValue) query = query.Where(t => t.IsOpen == isOpen);
			if (fromDate.HasValue) query = query.Where(t => t.CreatedDate >= fromDate);
			if (toDate.HasValue) query = query.Where(t => t.CreatedDate <= toDate);

			query = sortBy?.ToLower() switch
			{
				"ticketnumber" => sortDescending ? query.OrderByDescending(t => t.TicketNumber) : query.OrderBy(t => t.TicketNumber),
				"title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
				"createddate" => sortDescending ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate),
				_ => query.OrderByDescending(t => t.CreatedDate)
			};

			var totalCount = await query.CountAsync();
			var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

			return (items, totalCount);
		}

		public async Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId)
		{
			return await _dbSet
				.Include(t => t.Account)
				.Include(t => t.TicketType)
				.Include(t => t.TicketSubType)
				.Include(t => t.Priority)
				.Include(t => t.BusinessImpact)
				.Include(t => t.Status)
				.Include(t => t.CreatedByUser)
				.Include(t => t.AssignedToUser)
				.Include(t => t.RaisedByEmployee)
				.Include(t => t.Assignments).ThenInclude(a => a.AssignedToUser)
				.Include(t => t.Comments).ThenInclude(c => c.CommentedByUser)
				.Include(t => t.History).ThenInclude(h => h.ChangedByUser)
				.Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
				.FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);
		}

		public async Task<Ticket> CreateTicketAsync(Ticket ticket)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var store = await _context.TicketIDStores
					.FromSqlRaw("SELECT * FROM TicketIDStores WHERE Id = {0} FOR UPDATE", 1)
					.FirstAsync();

				store.CurrentNumber++;
				store.LastUpdatedDate = TimeHelper.GetIndianTime();

				ticket.TicketNumber = $"{store.Prefix}-{store.CurrentNumber:D6}";

				await _dbSet.AddAsync(ticket);
				await _context.SaveChangesAsync();

				await transaction.CommitAsync();
				return ticket;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task AddCommentAsync(TicketComment comment)
			=> await _context.TicketComments.AddAsync(comment);

		public async Task AddHistoryAsync(TicketHistory history)
			=> await _context.TicketHistories.AddAsync(history);

		public async Task<TicketStatus?> GetStatusByIdAsync(Guid statusId)
			=> await _context.TicketStatuses.FirstOrDefaultAsync(s => s.TicketStatusId == statusId);

		public async Task<TicketStatus?> GetStatusByNameAsync(string name)
			=> await _context.TicketStatuses
				.FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower() && !s.IsDeleted);

		public async Task<bool> ExistsAsync(Guid ticketId)
			=> await _dbSet.AnyAsync(t => t.TicketId == ticketId && !t.IsDeleted);

		public async Task<bool> HasOpenTicketsForAccountAsync(string accountId)
			=> await _dbSet.AnyAsync(t => t.AccountId == accountId && t.IsOpen && !t.IsDeleted);

		public async Task<TicketComment?> GetCommentAsync(Guid ticketId, Guid commentId)
			=> await _context.TicketComments
				.FirstOrDefaultAsync(c => c.TicketId == ticketId && c.TicketCommentId == commentId && !c.IsDeleted);

		public void UpdateComment(TicketComment comment)
			=> _context.TicketComments.Update(comment);
	}
}