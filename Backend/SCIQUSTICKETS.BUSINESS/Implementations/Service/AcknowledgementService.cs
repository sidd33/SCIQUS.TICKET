using System;
using System.Threading.Tasks;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class AcknowledgementService : IAcknowledgementService
    {
        private readonly ITicketEmailNotificationService _ticketEmailNotificationService;

        public AcknowledgementService(
            ITicketEmailNotificationService ticketEmailNotificationService)
        {
            _ticketEmailNotificationService =
                ticketEmailNotificationService;
        }

        public async Task HandleAsync(
            Guid ticketId,
            string eventType,
            string? actorUserId)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return;

            switch (eventType)
            {
				case "InProgress":
					await _ticketEmailNotificationService
						.SendCustomerStatusEmailAsync(
							ticketId,
							"In Progress");
					break;

				case "Closed":
					await _ticketEmailNotificationService
						.SendCustomerStatusEmailAsync(
							ticketId,
							"Closed");
					break;

				case "Reopened":
					await _ticketEmailNotificationService
						.SendCustomerStatusEmailAsync(
							ticketId,
							"Reopened");
					break;
			}
        }
    }
}