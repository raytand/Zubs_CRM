namespace Zubs.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<TreatmentRecord>? TreatmentRecords { get; set; }
    }
}
