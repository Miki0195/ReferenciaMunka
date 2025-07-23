using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// ReservationRequestDto
/// </summary>
public record ReservationRequestDto
{
    /// <summary>
    /// Name of the person making the reservation
    /// </summary>
    [StringLength(255, ErrorMessage = "Name is too long")]
    public required string Name { get; init; }

    /// <summary>
    /// Phone number of the person making the reservation
    /// </summary>
    [DataType(DataType.PhoneNumber, ErrorMessage = "Phone number is invalid")]
    [RegularExpression(@"^[0-9\-\+]{9,15}$", ErrorMessage = "Phone number is invalid")]
    public required string Phone { get; init; }

    /// <summary>
    /// Email of the person making the reservation
    /// </summary>
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public required string Email { get; init; }

    /// <summary>
    /// Additional comments for the reservation
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// The id of the screening
    /// </summary>
    public long ScreeningId { get; init; }

    /// <summary>
    /// Gets or Sets Seat
    /// </summary>
    public required List<SeatRequestDto> Seats { get; init; }
}
