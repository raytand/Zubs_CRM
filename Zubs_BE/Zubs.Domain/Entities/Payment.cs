using Zubs.Domain.Enums;

namespace Zubs.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public Guid AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateOnly PaidAt { get; set; }
        public PaymentMethod Method { get; set; } // Cash/Card/Online
    }
}
