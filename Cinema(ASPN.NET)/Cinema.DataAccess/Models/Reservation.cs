using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELTE.Cinema.DataAccess.Models;

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [MaxLength(15)]
    public string Phone { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Comment { get; set; }

    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = [];
}