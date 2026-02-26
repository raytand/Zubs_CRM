using System.ComponentModel.DataAnnotations;

namespace Zubs.Application.DTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string? Specialization { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
    public class DoctorUpdateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Specialization { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
    public class DoctorCreateDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Specialization { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

}
