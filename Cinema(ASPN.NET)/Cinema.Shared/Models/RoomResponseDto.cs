namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// RoomResponseDto
/// </summary>
public record RoomResponseDto
{
    /// <summary>
    /// Unique identifier for the room
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The name of the room
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Number of rows in the room
    /// </summary>
    public int Rows { get; init; }

    /// <summary>
    /// Number of columns in the room
    /// </summary>
    public int Columns { get; init; }
}
