using System.ComponentModel.DataAnnotations;
using Zubs.Domain.Enums;

namespace Zubs.Application.DTOs
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public Gender Gender { get; set; }
        public string? Notes { get; set; }
    }

    public class PatientCreateDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        [Phone]
        public string Phone { get; set; } = null!;
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public string? Notes { get; set; }
    }

    public class PatientUpdateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        [Phone] 
        public string Phone { get; set; } = null!;
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public string? Notes { get; set; }
    }
}
