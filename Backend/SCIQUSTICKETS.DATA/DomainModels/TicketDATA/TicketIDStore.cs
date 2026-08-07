using System;
using System.Collections.Generic;
using System.Text;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
	public class TicketIDStore
	{
		public int Id { get; set; }

		public string Prefix { get; set; } = "TKT";

		public long CurrentNumber { get; set; }

		public DateTime LastUpdatedDate { get; set; }
	}
}