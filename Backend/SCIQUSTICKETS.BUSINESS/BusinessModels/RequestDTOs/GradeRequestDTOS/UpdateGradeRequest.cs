using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class UpdateGradeRequest
	{
		public int? GradeLevel { get; set; }
		public string? Description { get; set; }
	}
}
