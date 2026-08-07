using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs
{
    public class CreateTicketPriorityRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "SlaInHours must be >= 0.")]
        public int SlaInHours { get; set; }
    }

    public class UpdateTicketPriorityRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "SlaInHours must be >= 0.")]
        public int SlaInHours { get; set; }

        public bool Status { get; set; }
    }
}
