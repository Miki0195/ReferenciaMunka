using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// MovieRequestDTO
/// </summary>
public record MovieRequestDto
{
    /// <summary>
    /// Title of the movie
    /// </summary>
    [MinLength(1, ErrorMessage = "Title shouldn't be empty")]
    [StringLength(255, ErrorMessage = "Title is too long")]
    public required string Title { get; init; }

    /// <summary>
    /// Year of release
    /// </summary>
    [Range(1895, int.MaxValue, ErrorMessage = "Year should be greater than 1895")]
    public int Year { get; init; }

    /// <summary>
    /// Director of the movie
    /// </summary>
    [MinLength(1, ErrorMessage = "Director name shouldn't be empty")]
    [StringLength(255, ErrorMessage = "Director name is too long")]
    public required string Director { get; init; }

    /// <summary>
    /// Short synopsis of the movie
    /// </summary>
    public required string Synopsis { get; init; }

    /// <summary>
    /// Length of the movie in minutes
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Length should be greater than 1")]
    public int Length { get; init; }

    /// <summary>
    /// Image or poster for the movie
    /// </summary>
    public byte[]? Image { get; init; }
}
