using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using SCIQUSTICKETS.DATA.DomainModels.DepartmentsDATA;

namespace SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA
{
	public class Employee
	{
		[Key, ForeignKey("ApplicationUser")]
		public string Id { get; set; }
		public string Name { get; set; }
		public string? RegisteredMobileNumber { get; set; }
		public string? SecondMobileNumber { get; set; }
		public string Email { get; set; }
		public string AutoGenrateId { get; set; }
		public string? EmployeeId { get; set; } // Employee-specific fields
												// here designation is not thier as designation is only the role field
		public string? Designation { get; set; }
		// Foreign Key to the ReportsToUser
		public string? ReportsTo { get; set; }
		[ForeignKey("ReportsTo")]
		public Employee ReportsToUser { get; set; }
		public Guid DepartmentId { get; set; }

		public Department Department { get; set; }

		//public ApplicationUser ApplicationUser { get; set; }

		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdatedDate { get; set; }

		public string? ProfileImageUrl { get; set; }

		// Collection of associated accounts
		//public ICollection<AccountAssignment> AccountAssignment { get; set; } = new List<AccountAssignment>();

		//public ICollection<OpportunityPhaseStatus> OpportunityPhaseStatuses { get; set; } = new List<OpportunityPhaseStatus>();

		public bool IsDeleted { get; set; } = false; // Add IsDeleted Property
		public Guid? GradeId { get; set; }
		[ForeignKey("GradeId")]
		public Grade? Grade { get; set; }

		public ICollection<EmployeeWorkingHour> WorkingHours { get; set; }
	= new List<EmployeeWorkingHour>();

		public ICollection<EmployeeLeave> Leaves { get; set; }
			= new List<EmployeeLeave>();


	}
}
