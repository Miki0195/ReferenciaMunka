using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// RoomRequestDTO
/// </summary>
public record RoomRequestDto
{
    /// <summary>
    /// The name of the room
    /// </summary>
    [MinLength(1, ErrorMessage = "Name shouldn't be empty")]
    [StringLength(255, ErrorMessage = "Name is too long")]
    public required string Name { get; init; }

    /// <summary>
    /// Number of rows in the room
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Rows should be greater than 1")]
    public int Rows { get; init; }

    /// <summary>
    /// Number of columns in the room
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Columns should be greater than 1")]
    public int Columns { get; init; }
}
