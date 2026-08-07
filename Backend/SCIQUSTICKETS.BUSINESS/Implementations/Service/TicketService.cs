using Microsoft.AspNetCore.Http;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class TicketService : ITicketService
	{
		private readonly ITicketRepository _ticketRepository;
		private readonly ITicketTypeRepository _ticketTypeRepository;
		private readonly ITicketSubTypeRepository _ticketSubTypeRepository;
		private readonly ITicketPriorityRepository _ticketPriorityRepository;
		private readonly ITicketBusinessImpactRepository _ticketBusinessImpactRepository;
		private readonly ITicketAttachmentRepository _ticketAttachmentRepository;
		private readonly IFileStorageService _fileStorageService;

		// Matches your actual seeded TicketStatus.Name values exactly (7-status model):
		// Open, In Progress, Pending, Resolved, PendingClosure, Closed, Reopened
		private static readonly Dictionary<string, string[]> AllowedTransitions =
			new(StringComparer.OrdinalIgnoreCase)
			{
				["Open"] = new[] { "In Progress", "Closed" },
				["In Progress"] = new[] { "Pending", "Resolved", "Closed" },
				["Pending"] = new[] { "In Progress", "Resolved", "Closed" },
				["Resolved"] = new[] { "PendingClosure", "In Progress", "Closed" },
				["PendingClosure"] = new[] { "Closed", "Reopened" },
				["Closed"] = new[] { "Reopened" },
				["Reopened"] = new[] { "In Progress" }
			};

		public TicketService(
			ITicketRepository ticketRepository,
			ITicketTypeRepository ticketTypeRepository,
			ITicketSubTypeRepository ticketSubTypeRepository,
			ITicketPriorityRepository ticketPriorityRepository,
			ITicketBusinessImpactRepository ticketBusinessImpactRepository,
			ITicketAttachmentRepository ticketAttachmentRepository,
			IFileStorageService fileStorageService)
		{
			_ticketRepository = ticketRepository;
			_ticketTypeRepository = ticketTypeRepository;
			_ticketSubTypeRepository = ticketSubTypeRepository;
			_ticketPriorityRepository = ticketPriorityRepository;
			_ticketBusinessImpactRepository = ticketBusinessImpactRepository;
			_ticketAttachmentRepository = ticketAttachmentRepository;
			_fileStorageService = fileStorageService;
		}

		public async Task<PagedResponse<TicketResponse>> GetAllAsync(TicketQueryParams queryParams)
		{
			var (items, totalCount) = await _ticketRepository.GetAllPagedAsync(
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
			var ticket = await _ticketRepository.GetByIdWithDetailsAsync(ticketId);
			return ticket == null ? null : MapToResponse(ticket);
		}

		public async Task<TicketResponse> CreateAsync(string userId, CreateTicketRequest request)
		{
			await ValidateReferencesAsync(request.TicketTypeId, request.TicketSubTypeId, request.PriorityId, request.BusinessImpactId);

			var now = TimeHelper.GetIndianTime();
			var openStatus = await _ticketRepository.GetStatusByNameAsync("Open")
				?? throw new InvalidOperationException("The 'Open' ticket status is not seeded.");

			var ticket = new Ticket
			{
				TicketId = Guid.NewGuid(),
				Title = request.Title,
				Description = request.Description,
				AccountId = request.AccountId,
				RaisedByEmployeeId = request.RaisedByEmployeeId,
				IsInternal = request.IsInternal,
				SourceType = request.SourceType,
				TicketTypeId = request.TicketTypeId,
				TicketSubTypeId = request.TicketSubTypeId,
				PriorityId = request.PriorityId,
				BusinessImpactId = request.BusinessImpactId,
				StatusId = openStatus.TicketStatusId,
				IsOpen = true,
				CreatedByUserId = userId,
				CreatedDate = now,
				LastUpdatedDate = now
			};

			var created = await _ticketRepository.CreateTicketAsync(ticket);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = created.TicketId,
				OldStatusId = null,
				NewStatusId = created.StatusId,
				ChangeDescription = "Ticket created",
				ChangedByUserId = userId,
				CreatedDate = now
			});
			await _ticketRepository.SaveChangesAsync();

			var full = await _ticketRepository.GetByIdWithDetailsAsync(created.TicketId);
			return MapToResponse(full ?? created);
		}

		private async Task ValidateReferencesAsync(Guid ticketTypeId, Guid ticketSubTypeId, Guid priorityId, Guid businessImpactId)
		{
			var ticketType = await _ticketTypeRepository.GetByIdAsync(ticketTypeId);
			if (ticketType is not TicketType tt || tt.IsDeleted || !tt.Status)
				throw new InvalidOperationException("The selected Ticket Type is invalid or inactive.");

			var subType = await _ticketSubTypeRepository.GetByIdAsync(ticketSubTypeId);
			if (subType is not TicketSubType st || st.IsDeleted || !st.Status)
				throw new InvalidOperationException("The selected Ticket Sub-Type is invalid or inactive.");
			if (st.TicketTypeId != ticketTypeId)
				throw new InvalidOperationException("The selected Sub-Type does not belong to the selected Ticket Type.");

			var priority = await _ticketPriorityRepository.GetByIdAsync(priorityId);
			if (priority is not TicketPriority p || p.IsDeleted || !p.Status)
				throw new InvalidOperationException("The selected Priority is invalid or inactive.");

			var impact = await _ticketBusinessImpactRepository.GetByIdAsync(businessImpactId);
			if (impact is not TicketBusinessTypeImpact i || i.IsDeleted || !i.Status)
				throw new InvalidOperationException("The selected Business Impact is invalid or inactive.");
		}

		public async Task<TicketResponse> UpdateAsync(Guid ticketId, UpdateTicketRequest request, string actorUserId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) throw new KeyNotFoundException("Ticket not found.");

			if (!ticket.IsOpen)
				throw new InvalidOperationException("A closed ticket cannot be edited. Reopen it first.");

			ticket.Title = request.Title;
			ticket.Description = request.Description;
			ticket.TicketTypeId = request.TicketTypeId;
			ticket.TicketSubTypeId = request.TicketSubTypeId;
			ticket.PriorityId = request.PriorityId;
			ticket.BusinessImpactId = request.BusinessImpactId;
			ticket.LastUpdatedDate = TimeHelper.GetIndianTime();

			_ticketRepository.Update(ticket);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				OldStatusId = null,
				NewStatusId = null,
				ChangeDescription = "Ticket details edited",
				ChangedByUserId = actorUserId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketRepository.SaveChangesAsync();

			var full = await _ticketRepository.GetByIdWithDetailsAsync(ticketId);
			return MapToResponse(full ?? ticket);
		}

		public async Task<bool> ChangeStatusAsync(Guid ticketId, ChangeTicketStatusRequest request, string userId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

			if (ticket.StatusId == request.StatusId)
				return true; // same-status ignored, not an error

			var currentStatus = await _ticketRepository.GetStatusByIdAsync(ticket.StatusId);
			var newStatus = await _ticketRepository.GetStatusByIdAsync(request.StatusId);

			if (currentStatus == null || newStatus == null)
				throw new InvalidOperationException("Invalid status reference.");

			if (!AllowedTransitions.TryGetValue(currentStatus.Name, out var validNext) ||
				!validNext.Contains(newStatus.Name, StringComparer.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(
					$"Cannot move ticket from '{currentStatus.Name}' to '{newStatus.Name}'.");
			}

			var oldStatusId = ticket.StatusId;

			ticket.StatusId = request.StatusId;
			ticket.IsOpen = !newStatus.IsClosed;
			ticket.LastUpdatedDate = TimeHelper.GetIndianTime();

			if (newStatus.Name.Equals("PendingClosure", StringComparison.OrdinalIgnoreCase))
			{
				ticket.PendingClosureDate = TimeHelper.GetIndianTime();
			}

			if (newStatus.Name.Equals("Closed", StringComparison.OrdinalIgnoreCase))
			{
				ticket.ClosureConfirmedBy ??= "Agent";
			}

			if (newStatus.Name.Equals("Reopened", StringComparison.OrdinalIgnoreCase))
			{
				ticket.PendingClosureDate = null;
				ticket.ClosureConfirmedBy = null;
			}

			_ticketRepository.Update(ticket);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				OldStatusId = oldStatusId,
				NewStatusId = request.StatusId,
				ChangeDescription = string.IsNullOrWhiteSpace(request.Comment)
					? $"Status changed from {currentStatus.Name} to {newStatus.Name}"
					: request.Comment,
				ChangedByUserId = userId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<bool> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, string userId)
		{
			var exists = await _ticketRepository.ExistsAsync(ticketId);
			if (!exists) return false;

			var comment = new TicketComment
			{
				TicketCommentId = Guid.NewGuid(),
				TicketId = ticketId,
				CommentText = request.Comment,
				CommentedByUserId = userId,
				IsInternalNote = request.IsInternalNote,
				CreatedDate = TimeHelper.GetIndianTime(),
				IsDeleted = false
			};

			await _ticketRepository.AddCommentAsync(comment);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = request.IsInternalNote ? "Internal note added" : "Comment added",
				ChangedByUserId = userId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteCommentAsync(Guid ticketId, Guid commentId, string actorUserId, bool canManageAll)
		{
			var comment = await _ticketRepository.GetCommentAsync(ticketId, commentId);
			if (comment == null) return false;

			if (!canManageAll && comment.CommentedByUserId != actorUserId)
				throw new UnauthorizedAccessException("Only the comment's author or a manage-all user can delete it.");

			comment.IsDeleted = true;
			_ticketRepository.UpdateComment(comment);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = "Comment deleted",
				ChangedByUserId = actorUserId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<bool> SoftDeleteAsync(Guid ticketId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

			ticket.IsDeleted = true;
			ticket.LastUpdatedDate = TimeHelper.GetIndianTime();

			_ticketRepository.Update(ticket);
			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<TicketAttachmentResponse> UploadAttachmentAsync(Guid ticketId, IFormFile file, string actorUserId)
		{
			var exists = await _ticketRepository.ExistsAsync(ticketId);
			if (!exists) throw new KeyNotFoundException("Ticket not found.");

			var (relativeUrl, originalName, size) = await _fileStorageService.SaveTicketFileAsync(ticketId, file);

			var attachment = new TicketAttachment
			{
				TicketAttachmentId = Guid.NewGuid(),
				TicketId = ticketId,
				FileName = originalName,
				FilePath = relativeUrl,
				FileSize = size,
				ContentType = file.ContentType,
				UploadedByUserId = actorUserId,
				UploadedDate = TimeHelper.GetIndianTime(),
				IsDeleted = false
			};

			await _ticketAttachmentRepository.AddAsync(attachment);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = $"Attachment added: {originalName}",
				ChangedByUserId = actorUserId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketAttachmentRepository.SaveChangesAsync();

			return MapAttachmentToResponse(attachment);
		}

		public async Task<IEnumerable<TicketAttachmentResponse>> GetAttachmentsAsync(Guid ticketId)
		{
			var items = await _ticketAttachmentRepository.GetByTicketIdAsync(ticketId);
			return items.Select(MapAttachmentToResponse);
		}

		public async Task<bool> DeleteAttachmentAsync(Guid ticketId, Guid attachmentId, string actorUserId, bool canManageAll)
		{
			var attachment = await _ticketAttachmentRepository.GetByIdForTicketAsync(ticketId, attachmentId);
			if (attachment == null) return false;

			if (!canManageAll && attachment.UploadedByUserId != actorUserId)
				throw new UnauthorizedAccessException("Only the uploader or a manage-all user can delete this attachment.");

			_fileStorageService.DeletePhysicalFile(attachment.FilePath);

			attachment.IsDeleted = true;
			_ticketAttachmentRepository.Update(attachment);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = $"Attachment removed: {attachment.FileName}",
				ChangedByUserId = actorUserId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketAttachmentRepository.SaveChangesAsync();
			return true;
		}

		private static TicketAttachmentResponse MapAttachmentToResponse(TicketAttachment a)
		{
			return new TicketAttachmentResponse
			{
				TicketAttachmentId = a.TicketAttachmentId,
				TicketId = a.TicketId,
				FileName = a.FileName,
				FilePath = a.FilePath,
				FileType = a.ContentType,
				FileSize = a.FileSize,
				UploadedByUserId = a.UploadedByUserId,
				UploadedByUserName = a.UploadedByUser?.UserName,
				UploadedDate = a.UploadedDate
			};
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

				RaisedByEmployeeId = t.RaisedByEmployeeId,
				RaisedByEmployeeName = t.RaisedByEmployee?.Name,

				IsInternal = t.IsInternal,
				SourceType = t.SourceType,

				TicketTypeId = t.TicketTypeId,
				TicketTypeName = t.TicketType?.Name ?? "",

				TicketSubTypeId = t.TicketSubTypeId,
				TicketSubTypeName = t.TicketSubType?.Name ?? "",

				PriorityId = t.PriorityId,
				PriorityName = t.Priority?.Name ?? "",

				BusinessImpactId = t.BusinessImpactId,
				BusinessImpactName = t.BusinessImpact?.Name ?? "",

				StatusId = t.StatusId,
				StatusName = t.Status?.Name ?? "",
				IsClosed = t.Status?.IsClosed ?? false,

				AssignedToUserId = t.AssignedToUserId,
				AssignedToUserName = t.AssignedToUser?.UserName,

				IsOpen = t.IsOpen,

				CreatedByUserId = t.CreatedByUserId,
				CreatedByUserName = t.CreatedByUser?.UserName,

				CreatedDate = t.CreatedDate,
				LastUpdatedDate = t.LastUpdatedDate
			};
		}
	}
}