using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// SeatRequestDto
/// </summary>
public record SeatRequestDto
{
    /// <summary>
    /// Row number of the seat
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Row should be greater than 1")]
    public int Row { get; init; }

    /// <summary>
    /// Column number of the seat
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Column should be greater than 1")]
    public int Column { get; init; }
}
