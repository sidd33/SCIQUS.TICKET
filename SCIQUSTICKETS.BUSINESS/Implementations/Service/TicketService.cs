using System;
using System.Collections.Generic;
using System.Text;

using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class TicketService : ITicketService
	{
		private readonly ITicketRepository _ticketRepository;

		public TicketService(
			ITicketRepository ticketRepository)
		{
			_ticketRepository = ticketRepository;
		}


		public async Task<PagedResponse<TicketResponse>> GetAllAsync(
	TicketQueryParams queryParams)
		{
			var (items, totalCount) =
				await _ticketRepository.GetAllPagedAsync(
					queryParams.Search,
					queryParams.TicketTypeId,
					queryParams.TicketSubTypeId,
					queryParams.PriorityId,
					queryParams.BusinessImpactId,
					queryParams.StatusId,
					queryParams.AccountId,
					queryParams.AssignedToUserId,
					queryParams.IsInternal,
					queryParams.IsOpen,
					queryParams.FromDate,
					queryParams.ToDate,
					queryParams.SortBy,
					queryParams.SortDescending,
					queryParams.Page,
					queryParams.PageSize);


			return new PagedResponse<TicketResponse>
			{
				Items = items.Select(MapToResponse).ToList(),
				TotalCount = totalCount,
				Page = queryParams.Page,
				PageSize = queryParams.PageSize
			};
		}



		public async Task<TicketResponse?> GetByIdAsync(Guid ticketId)
		{
			var ticket =
				await _ticketRepository.GetByIdWithDetailsAsync(ticketId);

			return ticket == null
				? null
				: MapToResponse(ticket);
		}



		public async Task<TicketResponse> CreateAsync(
			string userId,
			CreateTicketRequest request)
		{
			var ticketNumber =
				await _ticketRepository.GenerateTicketNumberAsync();


			var ticket = new Ticket
			{
				TicketId = Guid.NewGuid(),

				TicketNumber = ticketNumber,

				Title = request.Title,

				Description = request.Description,


				AccountId = request.AccountId,

				RaisedByEmployeeId =
					request.RaisedByEmployeeId,


				IsInternal = request.IsInternal,

				SourceType = request.SourceType,


				TicketTypeId = request.TicketTypeId,

				TicketSubTypeId =
					request.TicketSubTypeId,

				PriorityId = request.PriorityId,

				BusinessImpactId =
					request.BusinessImpactId,


				StatusId = Guid.Parse("10000000-0000-0000-0000-000000000001"),

				CreatedByUserId = userId,

				CreatedDate = DateTime.UtcNow,

				LastUpdatedDate = DateTime.UtcNow
			};


			await _ticketRepository.AddAsync(ticket);

			await _ticketRepository.SaveChangesAsync();


			return MapToResponse(ticket);
		}



		public async Task<TicketResponse> UpdateAsync(
			Guid ticketId,
			UpdateTicketRequest request)
		{
			var ticket =
				await _ticketRepository.GetByIdAsync(ticketId);


			if (ticket == null)
				throw new KeyNotFoundException();


			ticket.Title = request.Title;

			ticket.Description = request.Description;

			ticket.LastUpdatedDate = DateTime.UtcNow;


			_ticketRepository.Update(ticket);

			await _ticketRepository.SaveChangesAsync();


			return MapToResponse(ticket);
		}



		public async Task<bool> ChangeStatusAsync(
			Guid ticketId,
			ChangeTicketStatusRequest request,
			string userId)
		{
			var ticket =
				await _ticketRepository.GetByIdAsync(ticketId);


			if (ticket == null)
				return false;


			var oldStatus = ticket.StatusId;


			ticket.StatusId = request.StatusId;

			ticket.LastUpdatedDate =
				DateTime.UtcNow;



			var history = new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),

				TicketId = ticket.TicketId,

				OldStatusId = oldStatus,

				NewStatusId = request.StatusId,

				ChangeDescription =
					"Ticket status changed",

				ChangedByUserId = userId,

				CreatedDate = DateTime.UtcNow
			};


			await _ticketRepository.AddAsync(ticket);

			await _ticketRepository.SaveChangesAsync();


			return true;
		}



		public async Task<bool> AddCommentAsync(
			Guid ticketId,
			AddTicketCommentRequest request,
			string userId)
		{
			return await _ticketRepository.ExistsAsync(ticketId);
		}



		public async Task<bool> SoftDeleteAsync(Guid ticketId)
		{
			var ticket =
				await _ticketRepository.GetByIdAsync(ticketId);


			if (ticket == null)
				return false;


			ticket.IsDeleted = true;


			_ticketRepository.Update(ticket);

			await _ticketRepository.SaveChangesAsync();


			return true;
		}



		private static TicketResponse MapToResponse(Ticket t)
		{
			return new TicketResponse
			{
				TicketId = t.TicketId,

				TicketNumber = t.TicketNumber,

				Title = t.Title,

				Description = t.Description,


				AccountId = t.AccountId,

				AccountName = t.Account?.AccountName,

				IsInternal = t.IsInternal,

				SourceType = t.SourceType,


				TicketTypeId = t.TicketTypeId,

				TicketTypeName =
					t.TicketType?.Name ?? "",


				StatusId = t.StatusId,

				StatusName =
					t.Status?.Name ?? "",


				IsOpen = t.IsOpen,


				CreatedByUserId =
					t.CreatedByUserId,


				CreatedDate =
					t.CreatedDate,


				LastUpdatedDate =
					t.LastUpdatedDate
			};
		}
	}
}
