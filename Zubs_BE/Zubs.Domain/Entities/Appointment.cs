using Zubs.Domain.Enums;

namespace Zubs.Domain.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; } // Scheduled/Completed/Cancelled
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<TreatmentRecord>? TreatmentRecords { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
