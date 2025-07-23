namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// SeatResponseDto
/// </summary>
public record SeatResponseDto
{
    /// <summary>
    /// Unique identifier for the seat
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Row number of the seat
    /// </summary>
    public int Row { get; init; }

    /// <summary>
    /// Column number of the seat
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// Status of the seat (Reserved or Sold)
    /// </summary>
    public SeatStatusDto Status { get; init; }


    /// <summary>
    /// ReservationId of the seat (if reserved and not sold)
    /// </summary>
    public int? ReservationId { get; init; }
}
