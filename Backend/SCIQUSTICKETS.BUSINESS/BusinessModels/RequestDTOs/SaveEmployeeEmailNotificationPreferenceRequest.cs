namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs
{
	public class SaveEmployeeEmailNotificationPreferenceRequest
	{
		public bool ReceiveAll { get; set; }

		public bool Assignment { get; set; }

		public bool Acceptance { get; set; }

		public bool Rejection { get; set; }

		public bool Expiry { get; set; }

		public bool Reassignment { get; set; }

		public bool StatusChange { get; set; }

		public bool Closure { get; set; }

		public bool Reopen { get; set; }
	}
}