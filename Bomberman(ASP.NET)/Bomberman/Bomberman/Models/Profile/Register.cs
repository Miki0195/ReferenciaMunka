using System.ComponentModel.DataAnnotations;

namespace Bomberman.Models.Profile
{
    public class Register
    {

        [MaxLength(32, ErrorMessage = "Username must be at most 32 characters long!")]
        [Required(ErrorMessage = "Username is required.")]
        [NoWhitespace]
        [UniqueUsername]
        public string Username { get; set; } = null!;

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string PasswordAgain { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [UniqueEmail]
        public string Email { get; set; } = null!;
    }
}
