using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.DataAccess
{
    public class TravelAgencyContext : IdentityDbContext<User, IdentityRole, string>
	{
        public TravelAgencyContext() // Mockoláshoz szükséges
        { }

		public TravelAgencyContext(DbContextOptions<TravelAgencyContext> options)
			: base(options)
		{
		}

        protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
		}

        // Mockolás miatt fontos, hogy virtuálisak legyenek, így felüldefiniálhatók.

        public virtual DbSet<City> Cities { get; set; } = null!;
        public virtual DbSet<Building> Buildings { get; set; } = null!;
        public virtual DbSet<BuildingImage> BuildingImages { get; set; } = null!;
        public virtual DbSet<Apartment> Apartments { get; set; } = null!;
        public virtual DbSet<Rent> Rents { get; set; } = null!;
    }
}
