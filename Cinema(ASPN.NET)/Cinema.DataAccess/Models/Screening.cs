using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELTE.Cinema.DataAccess.Models;

public class Screening
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Movie")]
    public int MovieId { get; set; }

    [ForeignKey("Room")]
    public int RoomId { get; set; }
    
    public DateTime StartsAt { get; set; }
    
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = [];
}