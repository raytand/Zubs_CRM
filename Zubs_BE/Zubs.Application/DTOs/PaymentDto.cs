using Zubs.Domain.Enums;

namespace Zubs.Application.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidAt { get; set; }
        public PaymentMethod Method { get; set; }
    }

    public class PaymentCreateDto
    {
        public Guid PatientId { get; set; }
        public Guid AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidAt { get; set; }
        public PaymentMethod Method { get; set; }
    }

    public class PaymentUpdateDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidAt { get; set; }
        public PaymentMethod Method { get; set; }
    }
}
