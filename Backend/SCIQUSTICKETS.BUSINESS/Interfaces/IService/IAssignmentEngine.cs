using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Interfaces.IService
{
	public interface IAssignmentEngine
	{
		/// <summary>
		/// Resolves the target assignee for a ticket using the full 6-step auto-assignment pipeline:
		/// Step 0: Shortcut checks (Explicit Agent -> SubType Default Agent -> ManualOnly check)
		/// Step 1: Eligibility filter (Department active agents, MaxConcurrent, exclude rejectors/expirers)
		/// Step 2: Scoring Engine (RoundRobin, LoadBalanced, or Auto_assignment_custom multi-factor formula)
		/// Step 3: Anti-monopolization guard (MaxConsecutiveAssignments check)
		/// Step 4: Deterministic Tie-Break (HoursSinceLastAssigned desc, EmployeeId asc)
		/// Step 5: Return selected assignee or null (queue)
		/// </summary>
		Task<Employee?> ResolveAssigneeAsync(
			Ticket ticket,
			string? requestedAgentId = null,
			HashSet<string>? excludedAgentIds = null);
	}
}
