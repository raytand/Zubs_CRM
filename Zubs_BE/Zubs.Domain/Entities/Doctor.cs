namespace Zubs.Domain.Entities
{
    public class Doctor
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Specialization { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
    }
}
