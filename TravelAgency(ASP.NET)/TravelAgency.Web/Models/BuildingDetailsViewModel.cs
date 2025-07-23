using System.Collections.Generic;

namespace ELTE.TravelAgency.Web.Models
{
    public class BuildingDetailsViewModel : BuildingViewModel
    {
        public required ICollection<int> Images { get; set; }
    }
}
