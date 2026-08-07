using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class CreateGradeRequest
	{
		[Required]
		public int GradeLevel { get; set; }
		public string? Description { get; set; }
	}
}
