using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.Web.Models
{
    public class ReservationViewModel
    {
        /// <summary>
        /// Name of the person making the reservation
        /// </summary>
        [Required(ErrorMessage = "The name field is required.")]
        public required string Name { get; init; }

        /// <summary>
        /// Phone number of the person making the reservation
        /// </summary>
        [Required(ErrorMessage = "The phone number field is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public required string Phone { get; init; }

        /// <summary>
        /// Email of the person making the reservation
        /// </summary>
        [Required(ErrorMessage = "The email field is required.")]
        [EmailAddress(ErrorMessage = "The email address is not in the correct format.")]
        [DataType(DataType.EmailAddress)] // data type for validation
        public required string Email { get; init; }

        /// <summary>
        /// Additional comments for the reservation
        /// </summary>
        [StringLength(160, ErrorMessage = "The maximum length of the comment can be 160 characters.")]
        public string? Comment { get; init; }

        /// <summary>
        /// The ID of the screening
        /// </summary>
        public long ScreeningId { get; init; }

        /// <summary>
        /// Collection of seats belonging to the reservation
        /// </summary>
        public required List<SeatViewModel> Seats { get; init; }
    }
}
