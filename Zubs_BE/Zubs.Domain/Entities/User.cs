using System.Numerics;
using Zubs.Domain.Enums;

namespace Zubs.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; } // Admin / Doctor / Secretary
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        public ICollection<Doctor>? Doctors { get; set; }
        public ICollection<AuditLog>? AuditLogs { get; set; }
    }
}
