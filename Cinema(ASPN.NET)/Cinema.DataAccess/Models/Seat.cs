using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELTE.Cinema.DataAccess.Models;

public class Seat
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Screening")]
    public int ScreeningId { get; set; }

    public SeatPosition Position { get; set; } = null!;

    [Required]
    public SeatStatus Status { get; set; }

    [ForeignKey("Reservation")]
    public int? ReservationId { get; set; }

    public virtual Screening Screening { get; set; } = null!;

    public virtual Reservation? Reservation { get; set; }
}