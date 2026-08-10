using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.QueryParams;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
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
		private readonly IAssignmentEngine _assignmentEngine;
		private readonly AppDbContext _context;

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
			IFileStorageService fileStorageService,
			IAssignmentEngine assignmentEngine,
			AppDbContext context)
		{
			_ticketRepository = ticketRepository;
			_ticketTypeRepository = ticketTypeRepository;
			_ticketSubTypeRepository = ticketSubTypeRepository;
			_ticketPriorityRepository = ticketPriorityRepository;
			_ticketBusinessImpactRepository = ticketBusinessImpactRepository;
			_ticketAttachmentRepository = ticketAttachmentRepository;
			_fileStorageService = fileStorageService;
			_assignmentEngine = assignmentEngine;
			_context = context;
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

			var subType = await _ticketSubTypeRepository.GetByIdAsync(request.TicketSubTypeId);
			var priority = await _ticketPriorityRepository.GetByIdAsync(request.PriorityId);

			var now = TimeHelper.GetIndianTime();
			var openStatus = await _ticketRepository.GetStatusByNameAsync("Open")
				?? throw new InvalidOperationException("The 'Open' ticket status is not seeded.");

			// SLA Due Date calculation
			DateTime? slaDueDate = null;
			string? slaMetStatus = null;

			if (priority is TicketPriority p && p.SlaInHours > 0)
			{
				slaDueDate = now.AddHours(p.SlaInHours);
			}
			else
			{
				slaMetStatus = "Not Applicable";
			}

			// Inherit Department from SubType if not explicitly passed
			Guid departmentId = request.DepartmentId ?? subType!.DepartmentId;

			var ticket = new Ticket
			{
				TicketId = Guid.NewGuid(),
				Title = request.Title,
				Description = request.Description,
				AccountId = request.AccountId,
				RaisedByEmployeeId = request.RaisedByEmployeeId,
				IsInternal = request.IsInternal || string.IsNullOrEmpty(request.AccountId),
				SourceType = string.IsNullOrWhiteSpace(request.SourceType) ? "Portal" : request.SourceType,
				DepartmentId = departmentId,
				TicketTypeId = request.TicketTypeId,
				TicketSubTypeId = request.TicketSubTypeId,
				PriorityId = request.PriorityId,
				BusinessImpactId = request.BusinessImpactId,
				StatusId = openStatus.TicketStatusId,
				IsOpen = true,
				SlaDueDate = slaDueDate,
				SlaMetStatus = slaMetStatus,
				CreatedByUserId = userId,
				CreatedDate = now,
				LastUpdatedDate = now
			};

			// Auto-Assignment Resolution via IAssignmentEngine
			var assignedEmployee = await _assignmentEngine.ResolveAssigneeAsync(ticket, request.AssignedToUserId);
			if (assignedEmployee != null)
			{
				ticket.AssignedToUserId = assignedEmployee.Id;

				// Check acceptance requirement
				var globalConfig = await _context.SlaConfigurations.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive)
					?? new SlaConfiguration();

				bool requiresAcceptance = subType?.RequiresAcceptance ?? false;

				if (requiresAcceptance)
				{
					int deadlineHours = priority?.ResponseSlaInHours
						?? subType?.AcceptanceDeadlineHours
						?? 2;

					ticket.AcceptanceStatus = "Pending";
					ticket.AcceptanceDeadlineAt = now.AddHours(deadlineHours);
					ticket.AcceptanceDeadlineHours = deadlineHours;
				}
			}

			var created = await _ticketRepository.CreateTicketAsync(ticket);

			// Write Initial TicketAssignment row
			if (assignedEmployee != null)
			{
				await _context.TicketAssignments.AddAsync(new TicketAssignment
				{
					TicketAssignmentId = Guid.NewGuid(),
					TicketId = created.TicketId,
					AssignedToUserId = assignedEmployee.Id,
					AssignedByUserId = userId,
					AssignedDate = now,
					Status = "Assigned",
					IsAutoAssigned = string.IsNullOrWhiteSpace(request.AssignedToUserId)
				});
			}

			// Write Created TicketHistory row
			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = created.TicketId,
				OldStatusId = null,
				NewStatusId = created.StatusId,
				ChangeDescription = $"Ticket created. Assigned to: {(assignedEmployee?.Name ?? "Unassigned (Department Queue)")}",
				ChangedByUserId = userId,
				CreatedDate = now
			});

			await _context.SaveChangesAsync();

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
			var targetStatus = await _ticketRepository.GetStatusByIdAsync(request.StatusId);

			if (currentStatus == null || targetStatus == null)
				throw new InvalidOperationException("Invalid status identifier.");

			if (AllowedTransitions.TryGetValue(currentStatus.Name, out var allowed) &&
				!allowed.Contains(targetStatus.Name, StringComparer.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(
					$"Cannot transition ticket status from '{currentStatus.Name}' to '{targetStatus.Name}'.");
			}

			var now = TimeHelper.GetIndianTime();
			var oldStatusId = ticket.StatusId;

			ticket.StatusId = targetStatus.TicketStatusId;
			ticket.IsOpen = !string.Equals(targetStatus.Name, "Closed", StringComparison.OrdinalIgnoreCase);
			ticket.LastUpdatedDate = now;

			if (string.Equals(targetStatus.Name, "PendingClosure", StringComparison.OrdinalIgnoreCase))
			{
				ticket.PendingClosureDate = now;
			}
			else if (string.Equals(targetStatus.Name, "Closed", StringComparison.OrdinalIgnoreCase))
			{
				ticket.ClosureConfirmedBy ??= "Agent";
			}

			_ticketRepository.Update(ticket);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				OldStatusId = oldStatusId,
				NewStatusId = targetStatus.TicketStatusId,
				ChangeDescription = $"Status changed from {currentStatus.Name} to {targetStatus.Name}",
				ChangedByUserId = userId,
				CreatedDate = now
			});

			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<bool> ReassignAsync(Guid ticketId, AssignTicketRequest request, string actorUserId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

			if (!ticket.IsOpen)
				throw new InvalidOperationException("Cannot reassign a closed ticket.");

			var now = TimeHelper.GetIndianTime();
			var oldAssigneeId = ticket.AssignedToUserId;
			ticket.AssignedToUserId = request.AssignedToUserId;
			ticket.LastUpdatedDate = now;

			_ticketRepository.Update(ticket);

			await _context.TicketAssignments.AddAsync(new TicketAssignment
			{
				TicketAssignmentId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				AssignedToUserId = request.AssignedToUserId,
				AssignedByUserId = actorUserId,
				AssignedDate = now,
				Status = "Reassigned",
				IsAutoAssigned = false,
				Remarks = request.Remarks
			});

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				ChangeDescription = $"Reassigned to user: {request.AssignedToUserId}. Remarks: {request.Remarks}",
				ChangedByUserId = actorUserId,
				CreatedDate = now
			});

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> TransferDepartmentAsync(Guid ticketId, TransferTicketDepartmentRequest request, string actorUserId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

			if (!ticket.IsOpen)
				throw new InvalidOperationException("Cannot transfer a closed ticket.");

			var now = TimeHelper.GetIndianTime();
			var oldDeptId = ticket.DepartmentId;
			ticket.DepartmentId = request.DepartmentId;

			// Resolve new assignee in target department
			var targetAssignee = await _assignmentEngine.ResolveAssigneeAsync(ticket, request.TargetAssigneeUserId);
			ticket.AssignedToUserId = targetAssignee?.Id;
			ticket.LastUpdatedDate = now;

			_ticketRepository.Update(ticket);

			await _context.TicketAssignments.AddAsync(new TicketAssignment
			{
				TicketAssignmentId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				FromDepartmentId = oldDeptId,
				ToDepartmentId = request.DepartmentId,
				AssignedToUserId = targetAssignee?.Id ?? actorUserId,
				AssignedByUserId = actorUserId,
				AssignedDate = now,
				Status = "Transferred",
				IsAutoAssigned = string.IsNullOrWhiteSpace(request.TargetAssigneeUserId),
				Remarks = request.Comment
			});

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				ChangeDescription = $"Transferred to department: {request.DepartmentId}. Comment: {request.Comment}",
				ChangedByUserId = actorUserId,
				CreatedDate = now
			});

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> ChangePriorityImpactAsync(Guid ticketId, ChangePriorityImpactRequest request, string actorUserId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

			if (!ticket.IsOpen)
				throw new InvalidOperationException("Cannot edit priority/impact of a closed ticket.");

			var now = TimeHelper.GetIndianTime();
			string? oldPriorityName = ticket.Priority?.Name;

			if (request.PriorityId.HasValue)
			{
				var newPriority = await _ticketPriorityRepository.GetByIdAsync(request.PriorityId.Value);
				if (newPriority == null) throw new InvalidOperationException("Invalid priority ID.");

				ticket.PriorityId = request.PriorityId.Value;
				ticket.Priority = newPriority;

				// Recalculate SLA due date
				if (newPriority.SlaInHours > 0)
				{
					ticket.SlaDueDate = ticket.CreatedDate.AddHours(newPriority.SlaInHours);
					ticket.SlaMetStatus = null;
				}
				else
				{
					ticket.SlaDueDate = null;
					ticket.SlaMetStatus = "Not Applicable";
				}
			}

			if (request.BusinessImpactId.HasValue)
			{
				ticket.BusinessImpactId = request.BusinessImpactId.Value;
			}

			ticket.LastUpdatedDate = now;
			_ticketRepository.Update(ticket);

			await _context.TicketStateChangeHistories.AddAsync(new TicketStateChangeHistory
			{
				TicketStateChangeHistoryId = Guid.NewGuid(),
				TicketId = ticket.TicketId,
				ChangeType = request.PriorityId.HasValue ? "Priority" : "Impact",
				OldValue = oldPriorityName,
				NewValue = request.PriorityId.HasValue ? ticket.Priority?.Name : request.BusinessImpactId.ToString(),
				Reason = request.Reason,
				ChangedByUserId = actorUserId,
				CreatedDate = now
			});

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<PagedResponse<TicketResponse>> GetMyQueueAsync(string userId, TicketQueryParams queryParams)
		{
			queryParams.AssignedToUserId = userId;
			queryParams.IsOpen = true;
			return await GetAllAsync(queryParams);
		}

		public async Task<PagedResponse<TicketResponse>> GetDepartmentQueueAsync(string userId, TicketQueryParams queryParams)
		{
			var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == userId);
			if (employee == null) throw new UnauthorizedAccessException("Employee not found.");

			// Department queue includes all open tickets in employee's department
			queryParams.IsOpen = true;
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

			var deptItems = items.Where(t => t.DepartmentId == employee.DepartmentId).ToList();

			return new PagedResponse<TicketResponse>
			{
				Items = deptItems.Select(MapToResponse).ToList(),
				TotalCount = deptItems.Count,
				Page = queryParams.Page,
				PageSize = queryParams.PageSize
			};
		}

		public async Task<bool> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, string userId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

			var now = TimeHelper.GetIndianTime();
			var comment = new TicketComment
			{
				TicketCommentId = Guid.NewGuid(),
				TicketId = ticketId,
				CommentText = request.Comment,
				IsInternalNote = request.IsInternalNote,
				CommentedByUserId = userId,
				CreatedDate = now
			};

			await _ticketRepository.AddCommentAsync(comment);
			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = $"Comment added (Internal: {request.IsInternalNote})",
				ChangedByUserId = userId,
				CreatedDate = now
			});

			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteCommentAsync(Guid ticketId, Guid commentId, string actorUserId, bool canManageAll)
		{
			var comment = await _ticketRepository.GetCommentAsync(ticketId, commentId);
			if (comment == null) return false;

			if (!canManageAll && comment.CommentedByUserId != actorUserId)
				throw new UnauthorizedAccessException("Only the comment author or an admin can delete this comment.");

			comment.IsDeleted = true;
			_ticketRepository.UpdateComment(comment);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = "Comment soft-deleted",
				ChangedByUserId = actorUserId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _ticketRepository.SaveChangesAsync();
			return true;
		}

		public async Task<TicketAttachmentResponse> UploadAttachmentAsync(Guid ticketId, IFormFile file, string actorUserId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) throw new KeyNotFoundException("Ticket not found.");

			var (relativeUrl, originalFileName, fileSizeBytes) = await _fileStorageService.SaveTicketFileAsync(ticketId, file);
			var now = TimeHelper.GetIndianTime();

			var attachment = new TicketAttachment
			{
				TicketAttachmentId = Guid.NewGuid(),
				TicketId = ticketId,
				FileName = file.FileName,
				FilePath = relativeUrl,
				FileSize = file.Length,
				ContentType = file.ContentType,
				UploadedByUserId = actorUserId,
				UploadedDate = now
			};

			await _ticketAttachmentRepository.AddAsync(attachment);
			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = $"Attachment uploaded: {file.FileName}",
				ChangedByUserId = actorUserId,
				CreatedDate = now
			});

			await _context.SaveChangesAsync();

			return new TicketAttachmentResponse
			{
				TicketAttachmentId = attachment.TicketAttachmentId,
				FileName = attachment.FileName,
				FilePath = attachment.FilePath,
				FileSize = attachment.FileSize,
				UploadedByUserId = attachment.UploadedByUserId,
				UploadedDate = attachment.UploadedDate
			};
		}

		public async Task<IEnumerable<TicketAttachmentResponse>> GetAttachmentsAsync(Guid ticketId)
		{
			var attachments = await _ticketAttachmentRepository.GetByTicketIdAsync(ticketId);
			return attachments.Select(a => new TicketAttachmentResponse
			{
				TicketAttachmentId = a.TicketAttachmentId,
				FileName = a.FileName,
				FilePath = a.FilePath,
				FileSize = a.FileSize,
				UploadedByUserId = a.UploadedByUserId,
				UploadedDate = a.UploadedDate
			});
		}

		public async Task<bool> DeleteAttachmentAsync(Guid ticketId, Guid attachmentId, string actorUserId, bool canManageAll)
		{
			var attachment = await _ticketAttachmentRepository.GetByIdAsync(attachmentId);
			if (attachment == null || attachment.TicketId != ticketId) return false;

			if (!canManageAll && attachment.UploadedByUserId != actorUserId)
				throw new UnauthorizedAccessException("Only the uploader or an admin can delete this attachment.");

			_fileStorageService.DeletePhysicalFile(attachment.FilePath);
			_ticketAttachmentRepository.Delete(attachment);

			await _ticketRepository.AddHistoryAsync(new TicketHistory
			{
				TicketHistoryId = Guid.NewGuid(),
				TicketId = ticketId,
				ChangeDescription = $"Attachment deleted: {attachment.FileName}",
				ChangedByUserId = actorUserId,
				CreatedDate = TimeHelper.GetIndianTime()
			});

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> SoftDeleteAsync(Guid ticketId)
		{
			var ticket = await _ticketRepository.GetByIdAsync(ticketId);
			if (ticket == null) return false;

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
				RaisedByEmployeeId = t.RaisedByEmployeeId,
				RaisedByEmployeeName = t.RaisedByEmployee?.Name,
				IsInternal = t.IsInternal,
				SourceType = t.SourceType,
				DepartmentId = t.DepartmentId,
				DepartmentName = t.Department?.Name,
				IsOpen = t.IsOpen,
				TicketTypeId = t.TicketTypeId,
				TicketTypeName = t.TicketType?.Name,
				TicketSubTypeId = t.TicketSubTypeId,
				TicketSubTypeName = t.TicketSubType?.Name,
				PriorityId = t.PriorityId,
				PriorityName = t.Priority?.Name,
				PriorityLevel = t.Priority?.Level ?? 0,
				BusinessImpactId = t.BusinessImpactId,
				BusinessImpactName = t.BusinessImpact?.Description,
				StatusId = t.StatusId,
				StatusName = t.Status?.Name,
				CreatedByUserId = t.CreatedByUserId,
				CreatedByUserName = t.CreatedByUser?.UserName,
				AssignedToUserId = t.AssignedToUserId,
				AssignedToUserName = t.AssignedToUser?.UserName,
				SlaDueDate = t.SlaDueDate,
				SlaMetStatus = t.SlaMetStatus,
				AcceptanceStatus = t.AcceptanceStatus,
				AcceptanceDeadlineAt = t.AcceptanceDeadlineAt,
				CreatedDate = t.CreatedDate,
				LastUpdatedDate = t.LastUpdatedDate
			};
		}
	}
}