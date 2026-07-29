using System.ComponentModel.DataAnnotations;

namespace SCIQUSTICKETS.DATA.DomainModels
{
    public class Currency
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        public string CurrencyName { get; set; } = string.Empty;

        [Required]
        public string Symbol { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }
}
