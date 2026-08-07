using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SCIQUSTICKETS.DATA.DomainModels.EmployeeDATA
{
	public class Grade
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public int GradeLevel { get; set; }
		public string? Description { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
		public bool IsDeleted { get; set; } = false;
		public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
	}
}
