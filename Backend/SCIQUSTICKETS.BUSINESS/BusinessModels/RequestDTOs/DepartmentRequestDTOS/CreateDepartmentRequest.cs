using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class CreateDepartmentRequest
	{
		[Required, MaxLength(150)]
		public string Name { get; set; }

		public string? DepartmentHeadId { get; set; }
	}
}
