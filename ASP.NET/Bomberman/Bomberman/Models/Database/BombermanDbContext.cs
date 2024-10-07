using Microsoft.EntityFrameworkCore;

namespace Bomberman.Models.Database
{
    public class BombermanDbContext : DbContext
    {
        public BombermanDbContext(DbContextOptions<BombermanDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Score> Scores { get; set; }
    }
}
