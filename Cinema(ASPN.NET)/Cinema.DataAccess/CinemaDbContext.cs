using ELTE.Cinema.DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.DataAccess;

public class CinemaDbContext : IdentityDbContext<User, UserRole, string>
{
    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Screening> Screenings { get; set; } = null!;
    public DbSet<Seat> Seats { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Reservation)
            .WithMany(r => r.Seats)
            .HasForeignKey(s => s.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}