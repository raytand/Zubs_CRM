namespace Zubs.Application.DTOs
{
    public class MedicalRecordDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? Conditions { get; set; }
    }
    public class MedicalRecordCreateDto
    {
        public Guid PatientId { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? Conditions { get; set; }
    }
    public class MedicalRecordUpdateDto
    {
        public Guid Id { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? Conditions { get; set; }
    }
}

