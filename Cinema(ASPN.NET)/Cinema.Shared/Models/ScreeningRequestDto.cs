namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// ScreeningRequestDTO
/// </summary>
public record ScreeningRequestDto
{
    /// <summary>
    /// Gets or Sets MovieId
    /// </summary>
    public int MovieId { get; init; }

    /// <summary>
    /// Gets or Sets RoomId
    /// </summary>
    public int RoomId { get; init; }

    /// <summary>
    /// Start time of the screening
    /// </summary>
    public DateTime? StartsAt { get; init; }
}
