using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.DataAccess.Models;

public class Room
{
    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public int Rows { get; set; }
    public int Columns { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Screening> Screenings { get; set; } = [];
}