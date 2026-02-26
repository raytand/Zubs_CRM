using System.ComponentModel.DataAnnotations;
using Zubs.Domain.Enums;
namespace Zubs.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
    }
    public class UserCreateDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")] 
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; }
    }
    public class UserCredentialsDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class RegisterDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
