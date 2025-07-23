using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.Admin.Blazor.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Az email cím megadása kötelező.")]
        [EmailAddress(ErrorMessage = "Érvényes email címet adjon meg.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megadása kötelező.")]
        public string Password { get; set; } = string.Empty;
    }
}
