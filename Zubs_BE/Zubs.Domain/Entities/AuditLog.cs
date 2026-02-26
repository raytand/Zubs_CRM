namespace Zubs.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string Entity { get; set; } = null!; // Table name
        public Guid EntityId { get; set; }
        public string Action { get; set; } = null!; // Create/Update/Delete
        public Guid ChangedBy { get; set; } // User.Id
        public User? User { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
