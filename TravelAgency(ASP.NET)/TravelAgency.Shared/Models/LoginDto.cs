namespace ELTE.TravelAgency.Shared.Models
{
    public class LoginDto
    {
        /// <summary>
        /// Email.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Jelszó.
        /// </summary>
        public required string Password { get; set; }
    }
}
