using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels.TicketDATA
{
    public class TicketPriority
    {
        [Key]
        public Guid TicketPriorityId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        // Used for ordering / severity (e.g. 1 = highest)
        public int Level { get; set; }

        // The SLA time for tickets of this priority. Must be >= 0.
        // 0 means "no SLA applies" (Module 4: SlaDueDate = null, SlaMetStatus = "Not Applicable").
        public int SlaInHours { get; set; }

        // NOTE: ResponseSlaInHours (Module 12 acceptance-deadline source) is added later
        // by the AddTicketAutoRouting migration - not part of Module 1.

        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? LastUpdatedByUserId { get; set; }
    }
}
