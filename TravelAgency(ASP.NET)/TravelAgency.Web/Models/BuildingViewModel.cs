using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELTE.TravelAgency.Web.Models
{
    public class BuildingViewModel
    {
        public int Id { get; set; }

        [StringLength(255)]
        public required string Name { get; set; }

        public required CityViewModel City { get; set; }

        public int SeaDistance { get; set; }

        public ShoreTypeViewModel Shore { get; set; }

        public ICollection<FeatureViewModel> Features { get; set; } = [];

        public double LocationX { get; set; }

        public double LocationY { get; set; }

        [StringLength(1000)]
        public required string Comment { get; set; }

        public ICollection<ApartmentViewModel> Apartments { get; set; } = [];
    }
}
