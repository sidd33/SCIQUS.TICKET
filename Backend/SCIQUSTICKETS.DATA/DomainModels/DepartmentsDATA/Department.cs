using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA
{
	public class Department
	{
		[Key]
		public Guid DepartmentId { get; set; }
		public string Name { get; set; }


		public string? DepartmentHeadId { get; set; }
		public Employee DepartmentHead { get; set; }

		// Auto-Assignment Configuration per Department
		public string? TicketAutoAssignMethod { get; set; } // "RoundRobin", "LoadBalanced", "Auto_assignment_custom"
		public double? W_Load { get; set; }
		public double? W_Severity { get; set; }
		public double? W_Recency { get; set; }
		public int? RecencyCapHours { get; set; }
		public int? MaxConsecutiveAssignments { get; set; }
		public int? MaxConcurrentOpenTickets { get; set; }

		public bool IsDeleted { get; set; } = false;
		public DateTime CreatedDate { get; set; }
		public DateTime LastModifiedDate { get; set; }
		public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

	}
}
