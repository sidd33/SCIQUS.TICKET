// SCIQUSTICKETS.BUSINESS/Implementations/Service/FaqArticleService.cs
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class FaqArticleService : IFaqArticleService
	{
		private readonly AppDbContext _context;

		public FaqArticleService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<FaqArticleResponse>> GetAllAsync(bool includeInactive)
		{
			var query = _context.FaqArticles.AsNoTracking()
				.Include(f => f.TicketType)
				.Where(f => !f.IsDeleted);

			if (!includeInactive) query = query.Where(f => f.Status);

			var items = await query.OrderBy(f => f.Title).ToListAsync();
			return items.Select(MapToResponse);
		}

		public async Task<FaqArticleResponse?> GetByIdAsync(Guid id)
		{
			var entity = await _context.FaqArticles.AsNoTracking()
				.Include(f => f.TicketType)
				.FirstOrDefaultAsync(f => f.FaqArticleId == id && !f.IsDeleted);

			return entity == null ? null : MapToResponse(entity);
		}

		public async Task<FaqArticleResponse> CreateAsync(CreateFaqArticleRequest request, string actorUserId)
		{
			var now = TimeHelper.GetIndianTime();
			var entity = new FaqArticle
			{
				TicketTypeId = request.TicketTypeId,
				Title = request.Title,
				Body = request.Body,
				Status = true,
				IsDeleted = false,
				CreatedDate = now,
				LastUpdatedDate = now,
				CreatedByUserId = actorUserId
			};

			_context.FaqArticles.Add(entity);
			await _context.SaveChangesAsync();

			var created = await _context.FaqArticles.AsNoTracking()
				.Include(f => f.TicketType)
				.FirstAsync(f => f.FaqArticleId == entity.FaqArticleId);

			return MapToResponse(created);
		}

		public async Task<FaqArticleResponse?> UpdateAsync(Guid id, UpdateFaqArticleRequest request, string actorUserId)
		{
			var entity = await _context.FaqArticles.FirstOrDefaultAsync(f => f.FaqArticleId == id && !f.IsDeleted);
			if (entity == null) return null;

			entity.Title = request.Title;
			entity.Body = request.Body;
			entity.Status = request.Status;
			entity.LastUpdatedDate = TimeHelper.GetIndianTime();

			await _context.SaveChangesAsync();

			var updated = await _context.FaqArticles.AsNoTracking()
				.Include(f => f.TicketType)
				.FirstAsync(f => f.FaqArticleId == id);

			return MapToResponse(updated);
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			var entity = await _context.FaqArticles.FirstOrDefaultAsync(f => f.FaqArticleId == id && !f.IsDeleted);
			if (entity == null) return false;

			entity.IsDeleted = true;
			entity.LastUpdatedDate = TimeHelper.GetIndianTime();
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<FaqArticleResponse>> GetSuggestionsAsync(Guid ticketTypeId, Guid? ticketSubTypeId, string? query)
		{
			var articles = _context.FaqArticles.AsNoTracking()
				.Include(f => f.TicketType)
				.Where(f => !f.IsDeleted && f.Status && f.TicketTypeId == ticketTypeId);

			if (!string.IsNullOrWhiteSpace(query))
			{
				var trimmed = query.Trim();
				articles = articles.Where(f => f.Title.Contains(trimmed) || f.Body.Contains(trimmed));
			}

			if (ticketSubTypeId.HasValue)
			{
				articles = articles.Where(f =>
					f.TicketSubTypeId == ticketSubTypeId.Value ||
					f.TicketSubTypeId == null);
			}

			var items = await articles.OrderBy(f => f.Title).Take(5).ToListAsync();
			return items.Select(MapToResponse);
		}

		private static FaqArticleResponse MapToResponse(FaqArticle f) => new()
		{
			FaqArticleId = f.FaqArticleId,
			TicketTypeId = f.TicketTypeId,
			TicketTypeName = f.TicketType?.Name,
			Title = f.Title,
			Body = f.Body,
			Status = f.Status,
			CreatedDate = f.CreatedDate,
			LastUpdatedDate = f.LastUpdatedDate
		};
	}
}