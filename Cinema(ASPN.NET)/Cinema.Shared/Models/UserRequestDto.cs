using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.Shared.Models;

/// <summary>
/// UserRequestDTO
/// </summary>
public record UserRequestDto
{
    /// <summary>
    /// Name of the person making the reservation
    /// </summary>
    [StringLength(255, ErrorMessage = "Name is too long")]
    public required string Name { get; init; }

    /// <summary>
    /// Phone number of the person making the reservation
    /// </summary>
    [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
    [RegularExpression(@"^[0-9\-\+]{9,15}$", ErrorMessage = "Invalid Phone Number.")]
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Email of the person making the reservation
    /// </summary>
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public required string Email { get; init; }
    
    /// <summary>
    /// Password of the person making the reservation
    /// </summary>
    public required string Password { get; init; }

}
