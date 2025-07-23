using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.Shared.Models;

public class UserDto
{
    /// <summary>
    /// Felhasználó azonosítója
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Felhasználó neve.
    /// </summary>
    [Required(ErrorMessage = "A név megadása kötelező.")] // feltételek a validáláshoz
    [StringLength(60, ErrorMessage = "A foglaló neve maximum 60 karakter lehet.")]
    public required string Name { get; set; }

    /// <summary>
    /// Felhasználó e-mail címe.
    /// </summary>
    [Required(ErrorMessage = "E-mail cím megadása kötelező.")]
    [EmailAddress(ErrorMessage = "Az e-mail cím nem megfelelő formátumú.")]
    [DataType(DataType.EmailAddress)] // pontosítjuk az adatok típusát
    public required string Email { get; set; }

    /// <summary>
    /// Felhasználó címe.
    /// </summary>
    [Required(ErrorMessage = "A cím megadása kötelező.")]
    public required string Address { get; set; }

    /// <summary>
    /// Felhasználó telefonszáma.
    /// </summary>
    [Required(ErrorMessage = "A telefonszám megadása kötelező.")]
    [Phone(ErrorMessage = "A telefonszám formátuma nem megfelelő.")]
    [DataType(DataType.PhoneNumber)]
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Jelszó
    /// </summary>
    [Required(ErrorMessage = "A jelszó megadása kötelező.")]
    public string? Password { get; set; }
}