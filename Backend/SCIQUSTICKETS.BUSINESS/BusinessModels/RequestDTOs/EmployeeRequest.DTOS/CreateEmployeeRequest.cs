using System;
using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class CreateEmployeeRequest
	{
		[Required, MaxLength(200)]
		public string Name { get; set; }

		public string? RegisteredMobileNumber { get; set; }
		public string? SecondMobileNumber { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		[Required, MinLength(6)]
		public string Password { get; set; }

		public string? EmployeeId { get; set; }
		public string? Designation { get; set; }
		public string? ReportsTo { get; set; }

		[Required]
		public Guid DepartmentId { get; set; }

		public Guid? GradeId { get; set; }
		public string? ProfileImageUrl { get; set; }
	}
}