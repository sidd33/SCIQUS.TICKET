// SCIQUSTICKETS.BUSINESS/Interfaces/IService/IFaqArticleService.cs
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketMasterResponseDTOs;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IFaqArticleService
	{
		Task<IEnumerable<FaqArticleResponse>> GetAllAsync(bool includeInactive);
		Task<FaqArticleResponse?> GetByIdAsync(Guid id);
		Task<FaqArticleResponse> CreateAsync(CreateFaqArticleRequest request, string actorUserId);
		Task<FaqArticleResponse?> UpdateAsync(Guid id, UpdateFaqArticleRequest request, string actorUserId);
		Task<bool> SoftDeleteAsync(Guid id);

		/// <summary>
		/// Read-only suggestion lookup used during ticket creation. Never blocks creating a ticket.
		/// Matches articles linked to the given Type, optionally keyword-matched on Title/Body.
		/// </summary>
		Task<IEnumerable<FaqArticleResponse>> GetSuggestionsAsync(Guid ticketTypeId, Guid? ticketSubTypeId, string? query);
	}
}