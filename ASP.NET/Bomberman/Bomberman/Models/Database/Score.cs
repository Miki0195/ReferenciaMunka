using System.ComponentModel.DataAnnotations;

namespace Bomberman.Models.Database
{
    public class Score
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Points { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string? Context { get; set; } //Plus information here
        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
