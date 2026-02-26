namespace Zubs.Domain.Entities
{
    public class MedicalRecord
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? Conditions { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
