namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// ReservationResponseDto
/// </summary>
public record ReservationResponseDto
{
    /// <summary>
    /// Unique identifier for the reservation
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Name of the person making the reservation
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Phone number of the person making the reservation
    /// </summary>
    public required string Phone { get; init; }

    /// <summary>
    /// Email of the person making the reservation
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Date and time when the reservation was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Additional comments for the reservation
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Gets or Sets Seat
    /// </summary>

    public required List<SeatResponseDto> Seats { get; init; }
    
    /// <summary>
    /// Gets or Sets Screening
    /// </summary>
    public required ScreeningResponseDto Screening { get; init; }
}
