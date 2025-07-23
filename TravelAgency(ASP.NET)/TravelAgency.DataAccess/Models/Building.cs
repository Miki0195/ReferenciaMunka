using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.DataAccess.Models
{
    public class Building
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public required string Name { get; set; }
        public int CityId { get; set; }
        public int SeaDistance { get; set; }
        public ShoreType Shore { get; set; }
        public Feature Features { get; set; }
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        [MaxLength(1000)]
        public required string Comment { get; set; }

        public ICollection<Apartment> Apartments { get; set; } = [];
        public City City { get; set; } = null!;
    }
}
