using Bomberman.Models.Database;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Bomberman.Models.Profile
{
    public class Login
    {
        
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;

        public string? ReturnUrl { get; set; } = null!;

        public string? ErrorMessage { get; set; } = null!;
    }
}
