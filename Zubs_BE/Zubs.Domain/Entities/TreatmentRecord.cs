namespace Zubs.Domain.Entities
{
    public class TreatmentRecord
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public string? Notes { get; set; }
        public DateOnly PerformedAt { get; set; }
    }
}
