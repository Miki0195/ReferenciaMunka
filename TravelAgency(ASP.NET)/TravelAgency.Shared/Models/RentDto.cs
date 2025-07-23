using System;
using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.Shared.Models;

public class RentDto
{
    /// <summary>
    /// A foglalás azonosítója
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Apartman azonosítója
    /// </summary>
    public int ApartmentId { get; set; }

    /// <summary>
    /// Foglalás kezdete.
    /// </summary>
    [Required(ErrorMessage = "A kezdő dátum megadása kötelező.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Foglalás vége
    /// </summary>
    [Required(ErrorMessage = "A vége dátum megadása kötelező.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Vendég neve.
    /// </summary>
    [Required(ErrorMessage = "A név megadása kötelező.")] // feltételek a validáláshoz
    [StringLength(60, ErrorMessage = "A foglaló neve maximum 60 karakter lehet.")]
    public required string Name { get; set; }

    /// <summary>
    /// Vendég e-mail címe.
    /// </summary>
    [Required(ErrorMessage = "E-mail cím megadása kötelező.")]
    [EmailAddress(ErrorMessage = "Az e-mail cím nem megfelelő formátumú.")]
    [DataType(DataType.EmailAddress)] // pontosítjuk az adatok típusát
    public required string Email { get; set; }

    /// <summary>
    /// Vendég címe.
    /// </summary>
    [Required(ErrorMessage = "A cím megadása kötelező.")]
    public required string Address { get; set; }

    /// <summary>
    /// Vendég telefonszáma.
    /// </summary>
    [Required(ErrorMessage = "A telefonszám megadása kötelező.")]
    [Phone(ErrorMessage = "A telefonszám formátuma nem megfelelő.")]
    [DataType(DataType.PhoneNumber)]
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Teljes ár.
    /// </summary>
    [DataType(DataType.Currency)]
    public int TotalPrice { get; set; }
}