using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.ResponseDTOs
{
	public class GradeResponse
	{
		public Guid Id { get; set; }
		public int GradeLevel { get; set; }
		public string? Description { get; set; }
	}
}
