using Zubs.Domain.Enums;

namespace Zubs.Domain.Entities
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public ICollection<DentalChart>? DentalCharts { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }

}
