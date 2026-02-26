namespace Zubs.Domain.Entities
{
    public class DentalChart
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string ToothNumber { get; set; } = null!;
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
