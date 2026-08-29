using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.COMMON.Helpers;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.DATA.DomainModels.HolidayDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
	public class AssignmentEngine : IAssignmentEngine
	{
		private readonly AppDbContext _context;

		public AssignmentEngine(AppDbContext context)
		{
			_context = context;
		}

		public async Task<AssignmentExplanationResponse?> ResolveAssignmentExplanationAsync(
	Ticket ticket,
	string? requestedAgentId = null,
	HashSet<string>? excludedAgentIds = null)
		{
			var now = TimeHelper.GetIndianTime();
			var deptId = ticket.DepartmentId;

			var dept = await _context.Departments
				.AsNoTracking()
				.FirstOrDefaultAsync(d =>
					d.DepartmentId == deptId &&
					!d.IsDeleted);

			if (dept == null)
				return null;

			var subType = await _context.TicketSubTypes
				.AsNoTracking()
				.FirstOrDefaultAsync(st =>
					st.TicketSubTypeId == ticket.TicketSubTypeId &&
					!st.IsDeleted);

			var priority = await _context.TicketPriorities
				.AsNoTracking()
				.FirstOrDefaultAsync(p =>
					p.TicketPriorityId == ticket.PriorityId &&
					!p.IsDeleted);

			var globalConfig = await _context.SlaConfigurations
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.IsActive)
				?? new SlaConfiguration();

			// ============================================================
			// STEP 0 — Explicitly requested agent
			// ============================================================

			if (!string.IsNullOrWhiteSpace(requestedAgentId))
			{
				var requestedEmployee = await _context.Employees
					.AsNoTracking()
					.FirstOrDefaultAsync(e =>
						e.Id == requestedAgentId &&
						e.DepartmentId == deptId &&
						!e.IsDeleted);

				if (requestedEmployee != null &&
					await IsEmployeeAvailableAsync(requestedEmployee.Id, now))
				{
					return new AssignmentExplanationResponse
					{
						AssignedEmployeeId = requestedEmployee.Id,
						AssignedEmployeeName = requestedEmployee.Name,
						AssignmentType = "Manual",
						AssignmentMethod = "Explicit Agent",
						Reason = "The ticket was explicitly assigned to this employee.",
						IsAvailable = true,
						Candidates = new List<AssignmentCandidateExplanation>
				{
					new()
					{
						EmployeeId = requestedEmployee.Id,
						EmployeeName = requestedEmployee.Name,
						Selected = true
					}
				}
					};
				}
			}

			// ============================================================
			// STEP 0 — Sub-Type default agent
			// ============================================================

			if (subType != null &&
				!string.IsNullOrWhiteSpace(subType.DefaultUserId))
			{
				var defaultEmployee = await _context.Employees
					.AsNoTracking()
					.FirstOrDefaultAsync(e =>
						e.Id == subType.DefaultUserId &&
						e.DepartmentId == deptId &&
						!e.IsDeleted);

				if (defaultEmployee != null &&
					await IsEmployeeAvailableAsync(defaultEmployee.Id, now))
				{
					return new AssignmentExplanationResponse
					{
						AssignedEmployeeId = defaultEmployee.Id,
						AssignedEmployeeName = defaultEmployee.Name,
						AssignmentType = "Automatic",
						AssignmentMethod = "Sub-Type Default Agent",
						Reason =
							$"{defaultEmployee.Name} was selected because they are configured as the default agent for the '{subType.Name}' sub-type.",
						IsAvailable = true,
						Candidates = new List<AssignmentCandidateExplanation>
				{
					new()
					{
						EmployeeId = defaultEmployee.Id,
						EmployeeName = defaultEmployee.Name,
						Selected = true
					}
				}
					};
				}
			}

			// ============================================================
			// STEP 0 — Manual Only
			// ============================================================

			if ((priority != null && priority.ManualOnly) ||
				(subType != null && subType.ManualOnly))
			{
				return new AssignmentExplanationResponse
				{
					AssignmentType = "Manual",
					AssignmentMethod = "Manual Only",
					Reason =
						"The ticket was not automatically assigned because the selected priority or sub-type is configured as Manual Only."
				};
			}

			// ============================================================
			// STEP 1 — Eligibility
			// ============================================================

			int maxConcurrent =
				dept.MaxConcurrentOpenTickets
				?? globalConfig.DefaultMaxConcurrentOpenTickets;

			var activeEmployees = await _context.Employees
				.AsNoTracking()
				.Where(e =>
					e.DepartmentId == deptId &&
					!e.IsDeleted)
				.ToListAsync();

			if (excludedAgentIds != null && excludedAgentIds.Count > 0)
			{
				activeEmployees = activeEmployees
					.Where(e => !excludedAgentIds.Contains(e.Id))
					.ToList();
			}

			var availableEmployees = new List<Employee>();

			foreach (var employee in activeEmployees)
			{
				if (await IsEmployeeAvailableAsync(employee.Id, now))
				{
					availableEmployees.Add(employee);
				}
			}

			activeEmployees = availableEmployees;

			if (activeEmployees.Count == 0)
			{
				return new AssignmentExplanationResponse
				{
					AssignmentType = "Queue",
					Reason =
						"No eligible employees were available in the department, so the ticket was placed in the department queue."
				};
			}

			var agentOpenCounts = await _context.Tickets
				.AsNoTracking()
				.Where(t =>
					t.DepartmentId == deptId &&
					t.IsOpen &&
					!t.IsDeleted &&
					t.AssignedToUserId != null)
				.GroupBy(t => t.AssignedToUserId!)
				.Select(g => new
				{
					UserId = g.Key,
					Count = g.Count()
				})
				.ToDictionaryAsync(x => x.UserId, x => x.Count);

			var eligibleEmployees = activeEmployees
				.Where(e =>
					(agentOpenCounts.TryGetValue(e.Id, out int openCount)
						? openCount
						: 0) < maxConcurrent)
				.ToList();

			if (eligibleEmployees.Count == 0)
			{
				return new AssignmentExplanationResponse
				{
					AssignmentType = "Queue",
					Reason =
						"All available employees had reached their maximum concurrent ticket limit, so the ticket was placed in the department queue."
				};
			}

			// ============================================================
			// STEP 2 — Scoring
			// ============================================================

			string method =
				dept.TicketAutoAssignMethod
				?? globalConfig.DefaultAutoAssignMethod
				?? "LoadBalanced";

			double wLoad =
				dept.W_Load ??
				globalConfig.DefaultW_Load;

			double wSeverity =
				dept.W_Severity ??
				globalConfig.DefaultW_Severity;

			double wRecency =
				dept.W_Recency ??
				globalConfig.DefaultW_Recency;

			int recencyCapHours =
				dept.RecencyCapHours ??
				globalConfig.DefaultRecencyCapHours;

			var agentSeverityLoads = await _context.Tickets
				.AsNoTracking()
				.Where(t =>
					t.DepartmentId == deptId &&
					t.IsOpen &&
					!t.IsDeleted &&
					t.AssignedToUserId != null)
				.GroupBy(t => t.AssignedToUserId!)
				.Select(g => new
				{
					UserId = g.Key,
					SeveritySum = g.Sum(t => t.Priority.Level)
				})
				.ToDictionaryAsync(x => x.UserId, x => x.SeveritySum);

			var agentLastAssignments = await _context.TicketAssignments
				.AsNoTracking()
				.Where(ta =>
					ta.Ticket.DepartmentId == deptId &&
					!ta.IsDeleted)
				.GroupBy(ta => ta.AssignedToUserId)
				.Select(g => new
				{
					UserId = g.Key,
					MaxAssignedDate = g.Max(ta => ta.AssignedDate)
				})
				.ToDictionaryAsync(x => x.UserId, x => x.MaxAssignedDate);

			var scoredAgents = new List<ScoredAgent>();

			foreach (var emp in eligibleEmployees)
			{
				int openCount =
					agentOpenCounts.TryGetValue(emp.Id, out int oc)
						? oc
						: 0;

				int severityLoad =
					agentSeverityLoads.TryGetValue(emp.Id, out int sl)
						? sl
						: 0;

				double hoursSinceLastAssigned;

				if (agentLastAssignments.TryGetValue(
					emp.Id,
					out DateTime lastAssigned))
				{
					hoursSinceLastAssigned =
						(now - lastAssigned).TotalHours;

					if (hoursSinceLastAssigned < 0)
						hoursSinceLastAssigned = 0;
				}
				else
				{
					hoursSinceLastAssigned = recencyCapHours;
				}

				double effectiveRecencyHours =
					Math.Min(
						hoursSinceLastAssigned,
						recencyCapHours);

				double score;

				if (string.Equals(
					method,
					"RoundRobin",
					StringComparison.OrdinalIgnoreCase))
				{
					score = -effectiveRecencyHours;
				}
				else if (string.Equals(
					method,
					"LoadBalanced",
					StringComparison.OrdinalIgnoreCase))
				{
					score = openCount;
				}
				else
				{
					score =
						(wLoad * openCount) +
						(wSeverity * severityLoad) -
						(wRecency * effectiveRecencyHours);
				}

				scoredAgents.Add(new ScoredAgent
				{
					Employee = emp,
					Score = score,
					OpenCount = openCount,
					SeverityLoad = severityLoad,
					HoursSinceLastAssigned = hoursSinceLastAssigned
				});
			}

			scoredAgents = scoredAgents
				.OrderBy(sa => sa.Score)
				.ThenByDescending(sa => sa.HoursSinceLastAssigned)
				.ThenBy(sa => sa.Employee.Id)
				.ToList();

			// ============================================================
			// STEP 3 — Anti-monopolization
			// ============================================================

			var selectedAgent = scoredAgents[0];

			int maxConsecutive =
				dept.MaxConsecutiveAssignments
				?? globalConfig.DefaultMaxConsecutiveAssignments;

			bool antiMonopolizationApplied = false;

			if (scoredAgents.Count > 1 &&
				maxConsecutive > 0)
			{
				var recentAutoAssignees =
					await _context.TicketAssignments
						.AsNoTracking()
						.Where(ta =>
							ta.Ticket.DepartmentId == deptId &&
							ta.IsAutoAssigned &&
							!ta.IsDeleted)
						.OrderByDescending(ta => ta.AssignedDate)
						.Take(maxConsecutive)
						.Select(ta => ta.AssignedToUserId)
						.ToListAsync();

				if (recentAutoAssignees.Count == maxConsecutive &&
					recentAutoAssignees.All(
						id => id == selectedAgent.Employee.Id))
				{
					selectedAgent = scoredAgents[1];
					antiMonopolizationApplied = true;
				}
			}

			// ============================================================
			// Build explanation
			// ============================================================

			string reason;

			if (antiMonopolizationApplied)
			{
				reason =
					$"{selectedAgent.Employee.Name} was selected because the highest-ranked employee had received the previous {maxConsecutive} automatic assignments, so the anti-monopolization rule selected the next eligible employee.";
			}
			else if (string.Equals(
				method,
				"LoadBalanced",
				StringComparison.OrdinalIgnoreCase))
			{
				reason =
					$"{selectedAgent.Employee.Name} was selected because they had the lowest number of open tickets among the eligible employees.";
			}
			else if (string.Equals(
				method,
				"RoundRobin",
				StringComparison.OrdinalIgnoreCase))
			{
				reason =
					$"{selectedAgent.Employee.Name} was selected because they had been idle for the longest time among the eligible employees.";
			}
			else
			{
				reason =
					$"{selectedAgent.Employee.Name} was selected using the configured custom assignment scoring formula.";
			}

			return new AssignmentExplanationResponse
			{
				AssignedEmployeeId = selectedAgent.Employee.Id,
				AssignedEmployeeName = selectedAgent.Employee.Name,
				AssignmentType = "Automatic",
				AssignmentMethod = method,
				Reason = reason,
				Score = selectedAgent.Score,
				OpenTicketCount = selectedAgent.OpenCount,
				SeverityLoad = selectedAgent.SeverityLoad,
				HoursSinceLastAssignment = selectedAgent.HoursSinceLastAssigned,
				MaxConcurrentTickets = maxConcurrent,
				IsAvailable = true,

				AlgorithmName = method switch
				{
					"RoundRobin" => "Round Robin",
					"LoadBalanced" => "Load Balanced",
					_ => "Custom Formula"
				},

				Candidates = scoredAgents
					.Select(sa => new AssignmentCandidateExplanation
					{
						EmployeeId = sa.Employee.Id,
						EmployeeName = sa.Employee.Name,
						Score = sa.Score,
						OpenTicketCount = sa.OpenCount,
						SeverityLoad = sa.SeverityLoad,
						HoursSinceLastAssignment =
							sa.HoursSinceLastAssigned,
						Selected =
							sa.Employee.Id ==
							selectedAgent.Employee.Id
					})
					.ToList()
			};
		}

		public async Task<Employee?> ResolveAssigneeAsync(
			Ticket ticket,
			string? requestedAgentId = null,
			HashSet<string>? excludedAgentIds = null)
		{
			var now = TimeHelper.GetIndianTime();
			var deptId = ticket.DepartmentId;

			// Fetch Department details
			var dept = await _context.Departments
				.AsNoTracking()
				.FirstOrDefaultAsync(d => d.DepartmentId == deptId && !d.IsDeleted);

			if (dept == null) return null;

			// Fetch SubType details
			var subType = await _context.TicketSubTypes
				.AsNoTracking()
				.FirstOrDefaultAsync(st => st.TicketSubTypeId == ticket.TicketSubTypeId && !st.IsDeleted);

			// Fetch Priority details
			var priority = await _context.TicketPriorities
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.TicketPriorityId == ticket.PriorityId && !p.IsDeleted);

			// =========================================================================
			// STEP 0: Shortcut Checks
			// =========================================================================

			// 1. Explicitly requested agent
			if (!string.IsNullOrWhiteSpace(requestedAgentId))
			{
				var requestedEmployee = await _context.Employees
					.AsNoTracking()
					.FirstOrDefaultAsync(e =>
						e.Id == requestedAgentId &&
						e.DepartmentId == deptId &&
						!e.IsDeleted);

				if (requestedEmployee != null &&
	await IsEmployeeAvailableAsync(requestedEmployee.Id, now))
				{
					return requestedEmployee;
				}
			}

			// 2. Sub-Type Default Agent
			if (subType != null && !string.IsNullOrWhiteSpace(subType.DefaultUserId))
			{
				var defaultEmployee = await _context.Employees
					.AsNoTracking()
					.FirstOrDefaultAsync(e =>
						e.Id == subType.DefaultUserId &&
						e.DepartmentId == deptId &&
						!e.IsDeleted);

				if (defaultEmployee != null &&
	await IsEmployeeAvailableAsync(defaultEmployee.Id, now))
				{
					return defaultEmployee;
				}
			}

			// 3. Manual-Only check on Priority or SubType
			if ((priority != null && priority.ManualOnly) || (subType != null && subType.ManualOnly))
			{
				return null; // Go straight to department queue
			}

			// =========================================================================
			// STEP 1: Eligibility Filter
			// =========================================================================

			var globalConfig = await _context.SlaConfigurations
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.IsActive)
				?? new SlaConfiguration();

			int maxConcurrent = dept.MaxConcurrentOpenTickets
				?? globalConfig.DefaultMaxConcurrentOpenTickets;

			// Active employees in department
			var activeEmployees = await _context.Employees
				.AsNoTracking()
				.Where(e => e.DepartmentId == deptId && !e.IsDeleted)
				.ToListAsync();

			// --- Support Plan Dedicated Agent Routing ---
			if (!string.IsNullOrEmpty(ticket.AccountId) && !ticket.IsInternal)
			{
				var activePlan = await _context.AccountSupportPlans
					.Include(asp => asp.SupportPlan)
					.AsNoTracking()
					.Where(asp => asp.AccountId == ticket.AccountId && asp.Status == "Active" && 
								  asp.StartDate <= now && asp.EndDate >= now)
					.Select(asp => asp.SupportPlan)
					.FirstOrDefaultAsync();

				if (activePlan != null && (activePlan.AssignmentStrategy == "AllocatedGroup" || activePlan.AssignmentStrategy == "DedicatedPrimary"))
				{
					var dedicatedEmployeeIds = await _context.AccountDedicatedEmployees
						.AsNoTracking()
						.Where(ade => ade.AccountId == ticket.AccountId)
						.Select(ade => ade.EmployeeUserId)
						.ToListAsync();

					if (dedicatedEmployeeIds.Any())
					{
						// Restrict activeEmployees to ONLY those who are dedicated to this account
						activeEmployees = activeEmployees
							.Where(e => dedicatedEmployeeIds.Contains(e.Id))
							.ToList();
					}
				}
			}
			// --------------------------------------------

			if (excludedAgentIds != null && excludedAgentIds.Count > 0)
			{
				activeEmployees = activeEmployees
					.Where(e => !excludedAgentIds.Contains(e.Id))
					.ToList();
			}

			// Remove employees who are on leave or outside working hours
			var availableEmployees = new List<Employee>();

			foreach (var employee in activeEmployees)
			{
				if (await IsEmployeeAvailableAsync(employee.Id, now))
				{
					availableEmployees.Add(employee);
				}
			}

			activeEmployees = availableEmployees;

			if (activeEmployees.Count == 0)
				return null;
			// Compute open ticket count per agent to check maxConcurrent
			var agentOpenCounts = await _context.Tickets
				.AsNoTracking()
				.Where(t => t.DepartmentId == deptId && t.IsOpen && !t.IsDeleted && t.AssignedToUserId != null)
				.GroupBy(t => t.AssignedToUserId!)
				.Select(g => new { UserId = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.UserId, x => x.Count);

			var eligibleEmployees = activeEmployees
				.Where(e => (agentOpenCounts.TryGetValue(e.Id, out int openCount) ? openCount : 0) < maxConcurrent)
				.ToList();

			if (eligibleEmployees.Count == 0)
				return null;

			if (eligibleEmployees.Count == 1)
				return eligibleEmployees[0];

			// =========================================================================
			// STEP 2: Calculate Metrics & Formula Scoring
			// =========================================================================

			string method = dept.TicketAutoAssignMethod
				?? globalConfig.DefaultAutoAssignMethod
				?? "LoadBalanced";

			double wLoad = dept.W_Load ?? globalConfig.DefaultW_Load;
			double wSeverity = dept.W_Severity ?? globalConfig.DefaultW_Severity;
			double wRecency = dept.W_Recency ?? globalConfig.DefaultW_Recency;
			int recencyCapHours = dept.RecencyCapHours ?? globalConfig.DefaultRecencyCapHours;
			int maxConsecutive = dept.MaxConsecutiveAssignments ?? globalConfig.DefaultMaxConsecutiveAssignments;

			// Gather open ticket severity sum per agent
			var agentSeverityLoads = await _context.Tickets
				.AsNoTracking()
				.Where(t => t.DepartmentId == deptId && t.IsOpen && !t.IsDeleted && t.AssignedToUserId != null)
				.GroupBy(t => t.AssignedToUserId!)
				.Select(g => new { UserId = g.Key, SeveritySum = g.Sum(t => t.Priority.Level) })
				.ToDictionaryAsync(x => x.UserId, x => x.SeveritySum);

			// Gather last assignment date per agent
			var agentLastAssignments = await _context.TicketAssignments
				.AsNoTracking()
				.Where(ta => ta.Ticket.DepartmentId == deptId && !ta.IsDeleted)
				.GroupBy(ta => ta.AssignedToUserId)
				.Select(g => new { UserId = g.Key, MaxAssignedDate = g.Max(ta => ta.AssignedDate) })
				.ToDictionaryAsync(x => x.UserId, x => x.MaxAssignedDate);

			var scoredAgents = new List<ScoredAgent>();

			foreach (var emp in eligibleEmployees)
			{
				int openCount = agentOpenCounts.TryGetValue(emp.Id, out int oc) ? oc : 0;
				int severityLoad = agentSeverityLoads.TryGetValue(emp.Id, out int sl) ? sl : 0;
				
				double hoursSinceLastAssigned;
				if (agentLastAssignments.TryGetValue(emp.Id, out DateTime lastAssigned))
				{
					hoursSinceLastAssigned = (now - lastAssigned).TotalHours;
					if (hoursSinceLastAssigned < 0) hoursSinceLastAssigned = 0;
				}
				else
				{
					hoursSinceLastAssigned = recencyCapHours; // Maximally idle if never assigned
				}

				double effectiveRecencyHours = Math.Min(hoursSinceLastAssigned, recencyCapHours);

				double score;
				if (string.Equals(method, "RoundRobin", StringComparison.OrdinalIgnoreCase))
				{
					score = -effectiveRecencyHours; // Most idle (largest recency) gets lowest score
				}
				else if (string.Equals(method, "LoadBalanced", StringComparison.OrdinalIgnoreCase))
				{
					score = openCount;
				}
				else // "Auto_assignment_custom" or default
				{
					score = (wLoad * openCount) + (wSeverity * severityLoad) - (wRecency * effectiveRecencyHours);
				}

				scoredAgents.Add(new ScoredAgent
				{
					Employee = emp,
					Score = score,
					OpenCount = openCount,
					SeverityLoad = severityLoad,
					HoursSinceLastAssigned = hoursSinceLastAssigned
				});
			}

			// Sort by Score ascending
			scoredAgents = scoredAgents
				.OrderBy(sa => sa.Score)
				.ThenByDescending(sa => sa.HoursSinceLastAssigned)
				.ThenBy(sa => sa.Employee.Id)
				.ToList();

			// =========================================================================
			// STEP 3: Anti-Monopolization Guard
			// =========================================================================

			if (scoredAgents.Count > 1 && maxConsecutive > 0)
			{
				var recentAutoAssignees = await _context.TicketAssignments
					.AsNoTracking()
					.Where(ta => ta.Ticket.DepartmentId == deptId && ta.IsAutoAssigned && !ta.IsDeleted)
					.OrderByDescending(ta => ta.AssignedDate)
					.Take(maxConsecutive)
					.Select(ta => ta.AssignedToUserId)
					.ToListAsync();

				if (recentAutoAssignees.Count == maxConsecutive &&
					recentAutoAssignees.All(id => id == scoredAgents[0].Employee.Id))
				{
					// Top-ranked agent has monopolized the last N assignments -> pick second best
					return scoredAgents[1].Employee;
				}
			}

			// =========================================================================
			// STEP 4 & 5: Return Selected Agent
			// =========================================================================

			return scoredAgents[0].Employee;
		}

		private async Task<bool> IsEmployeeAvailableAsync(string employeeId, DateTime now)
		{
			var employee = await _context.Employees
				.AsNoTracking()
				.FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

			if (employee == null)
				return false;

			// ============================================================
			// CHECK 0: Company holiday
			// ============================================================

			var today = now.Date;

			var holidayToday = await _context.Holidays
				.AsNoTracking()
				.FirstOrDefaultAsync(h => h.Date.Date == today && !h.IsDeleted);

			if (holidayToday != null)
			{
				var confirmation = await _context.HolidayConfirmations
					.AsNoTracking()
					.FirstOrDefaultAsync(c =>
						c.HolidayId == holidayToday.Id &&
						c.EmployeeId == employeeId &&
						!c.IsDeleted);

				bool confirmedAvailable = confirmation != null &&
					confirmation.Status == "Available";

				if (!confirmedAvailable)
					return false;
			}

			// ============================================================
			// CHECK 1: Approved leave
			// ============================================================


			var onLeave = await _context.EmployeeLeaves
			.AsNoTracking()
			.AnyAsync(l =>
				l.EmployeeId == employeeId &&
				!l.IsDeleted &&
				l.Status == "Approved" &&
				l.StartDate.Date <= today &&
				l.EndDate.Date >= today);

			if (onLeave)
				return false;

			// ============================================================
			// CHECK 2: Working hours
			// ============================================================

			var currentDay = now.DayOfWeek;
			var currentTime = now.TimeOfDay;

			var workingHour = await _context.EmployeeWorkingHours
				.AsNoTracking()
				.FirstOrDefaultAsync(w =>
					w.EmployeeId == employeeId &&
					w.DayOfWeek == currentDay &&
					w.IsWorkingDay);

			if (workingHour == null)
				return false;

			// Normal working period
			if (workingHour.StartTime <= workingHour.EndTime)
			{
				return currentTime >= workingHour.StartTime &&
					   currentTime <= workingHour.EndTime;
			}

			// Handles overnight shifts such as 22:00 - 06:00
			return currentTime >= workingHour.StartTime ||
				   currentTime <= workingHour.EndTime;
		}

		private class ScoredAgent
		{
			public Employee Employee { get; set; } = null!;
			public double Score { get; set; }
			public int OpenCount { get; set; }
			public int SeverityLoad { get; set; }
			public double HoursSinceLastAssigned { get; set; }
		}
	}
}
