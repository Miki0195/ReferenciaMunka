namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// UserResponseDTO
/// </summary>
public record UserResponseDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name of the person making the reservation
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Phone number of the person making the reservation
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Email of the person making the reservation
    /// </summary>
    public required string Email { get; init; }
}
