using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class SetDepartmentHeadRequest
	{
		[Required]
		public string DepartmentHeadId { get; set; }
	}
}
