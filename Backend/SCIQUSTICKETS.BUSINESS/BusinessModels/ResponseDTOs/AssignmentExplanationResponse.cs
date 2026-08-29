using System.Collections.Generic;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs.TicketResponseDTOs
{
	public class AssignmentExplanationResponse
	{
		public string? AssignedEmployeeId { get; set; }
		public string? AssignedEmployeeName { get; set; }

		public string AssignmentType { get; set; } = string.Empty;

		public string Reason { get; set; } = string.Empty;

		public string? AssignmentMethod { get; set; }

		public double Score { get; set; }

		public int OpenTicketCount { get; set; }

		public int SeverityLoad { get; set; }

		public double HoursSinceLastAssignment { get; set; }

		public int MaxConcurrentTickets { get; set; }

		public bool IsAvailable { get; set; }

		public bool IsOnLeave { get; set; }

		public bool IsWithinWorkingHours { get; set; }

		public List<AssignmentCandidateExplanation> Candidates { get; set; }
			= new();


		public string? Context { get; set; }
		public string? ShortReason { get; set; }
		public string? AlgorithmName { get; set; }  
	}

	public class AssignmentCandidateExplanation
	{
		public string EmployeeId { get; set; } = string.Empty;

		public string EmployeeName { get; set; } = string.Empty;

		public double Score { get; set; }

		public int OpenTicketCount { get; set; }

		public int SeverityLoad { get; set; }

		public double HoursSinceLastAssignment { get; set; }

		public bool Selected { get; set; }
	}
}